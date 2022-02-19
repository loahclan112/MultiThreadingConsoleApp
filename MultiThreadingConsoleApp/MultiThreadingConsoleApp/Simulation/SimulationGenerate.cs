using MultiThreadingConsoleApp.Utilities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MultiThreadingConsoleApp.Simulation
{

    public class SimulationGenerate : SimulationBase
    {
        public int MapXMax = 150;
        public bool isZombieModeOn;
        public  Data data;
        public  List<StatusEnum> infectedPeopleStart;
        public  Task timerThread;
        public  Task printerThread;


        public SimulationGenerate(int threadCount = 1, int MapXMax = 150, int MapYMax = 75, int PeopleCount = 20, bool isMapVisible = true, bool isZombieModeOn = true, int InfectedCount = 1 )
        {
            this.ThreadCount = threadCount;
            this.IsMapVisible = isMapVisible;
            this.MapXMax = MapXMax;
            this.MapYMax = MapYMax;
            this.PeopleCount = PeopleCount;
            this.isZombieModeOn = isZombieModeOn;
            this.InfectedCount = InfectedCount;
        }

        public override void StartSimulation()
        {
          
            Console.CursorVisible = false;
            timerThread = new Task(StartTimer, Source.Token, TaskCreationOptions.LongRunning);
            printerThread = new Task(StartPrinter, Source.Token, TaskCreationOptions.LongRunning);

            IsDoneWithMovement = new List<bool>();

            Map = new Map(MapXMax, MapYMax);

            data = new Data();
            data.MapX = MapXMax;
            data.MapY = MapYMax;


            GlobalPersonDictionary = InitPersonDictionaryRandom(PeopleCount);

            InfectRandomPeople(GlobalPersonDictionary, InfectedCount);

            infectedPeopleStart = new List<StatusEnum>();

            foreach (var item in GlobalPersonDictionary.Values)
            {
                infectedPeopleStart.Add(item.Status);
            }

            List<ConcurrentDictionary<int, Person>> listDictionary = new List<ConcurrentDictionary<int, Person>>();

            Dictionary<int, Person> personDictionarytemp = new Dictionary<int, Person>();

            int personDistributedCount = 0;

            for (int i = 0; i < ThreadCount; i++)
            {
                for (int j = 0; j < PeopleCount / ThreadCount; j++)
                {
                    personDictionarytemp.Add(i * (PeopleCount / ThreadCount) + j, GlobalPersonDictionary[i * (PeopleCount / ThreadCount) + j]);
                    personDistributedCount++;
                }

                listDictionary.Add(new ConcurrentDictionary<int, Person>(personDictionarytemp));
                personDictionarytemp = new Dictionary<int, Person>();
            }

            for (int k = 1; k <= PeopleCount - personDistributedCount; k++)
            {

                listDictionary[ThreadCount - 1].TryAdd(PeopleCount - k, GlobalPersonDictionary[PeopleCount - k]);

            }
            int index = 1;
            foreach (var item in listDictionary)
            {
                int j = index;
                IsDoneWithMovement.Add(false);
                if (isZombieModeOn)
                {
                    Threads.Add(new Task(() => ThreadMethodZombie(item, GlobalPersonDictionary), TaskCreationOptions.LongRunning));

                }
                else {
                    Threads.Add(new Task(() => ThreadMethod(item), TaskCreationOptions.LongRunning));

                }
                index++;

            }

            foreach (var item in Threads)
            {
                item.Start();
            }

            timerThread.Start();
            printerThread.Start();
            Console.ReadLine();
        }


        public override void EndCheck()
        {
            if (Source.IsCancellationRequested)
            {
                data.PersonList = GlobalPersonDictionary.Values.ToList();
                for (int i = 0; i < infectedPeopleStart.Count; i++)
                {
                    data.PersonList[i].Status = infectedPeopleStart[i];
                }
                FileHandler.WriteToFile(data.SaveContent(), true);
                Person.commonId = 0;
                IsDone = true;
            }
        }

        public void ThreadMethodZombie(ConcurrentDictionary<int, Person> personDictionary, ConcurrentDictionary<int, Person> GlobalPersonDictionary)
        {
            int index = Threads.FindIndex(x => x.Id == Task.CurrentId);
            while (InfectedCount != PeopleCount)
            {
                IsDoneWithMovement[index] = false;
                MovePeopleZombie(personDictionary, GlobalPersonDictionary);
                IsDoneWithMovement[index] = true;

                while (!IsPrinterDone)
                {

                }
            }
            Source.Cancel();
            Task.WaitAll(Threads.ToArray());
            Threads[index].Dispose();
        }

        public void InfectRandomPeople(ConcurrentDictionary<int, Person> personDictionary, int count)
        {
            List<int> alreadyInfected = new List<int>();

            count = Math.Min(count, PeopleCount);

            int id;

            while (alreadyInfected.Count != count)
            {
                id = RandomGenerator.Generate(count);
                if (!alreadyInfected.Contains(id))
                {
                    personDictionary[id].Status = StatusEnum.Infected;
                    alreadyInfected.Add(id);
                }
            }

        }

        public void MovePeopleZombie(ConcurrentDictionary<int, Person> personDictionary, ConcurrentDictionary<int, Person> GlobalPersonDictionary )
        {
            foreach (var item in personDictionary)
            {
                Thinking();
                if (item.Value.Status != StatusEnum.Infected)
                {
                    RandomMove(item.Value, MapXMax, MapYMax);
                }
                else 
                {
                    ZombieMove(item.Value, GlobalPersonDictionary, MapXMax, MapYMax);
                }
                item.Value.DonePositions.Add(item.Value.Position);
            }

        }

        public override void Thinking()
        {

        }

        public override void MovePeople(ConcurrentDictionary<int, Person> personDictionary)
        {
            foreach (var item in personDictionary)
            {
                Thinking();
                RandomMove(item.Value, MapXMax, MapYMax);
                item.Value.DonePositions.Add(item.Value.Position);

            }

        }

        public void ZombieMove(Person p, ConcurrentDictionary<int, Person> GlobalPersonDictionary, int MapXMax, int MapYMax)
        {
            Person closestNotInfectedPerson;

            double closestDistance = double.MaxValue;

            List<Person> notInfectedPersonList = GlobalPersonDictionary.Values.Where(x => x.Status != StatusEnum.Infected).ToList();

            if (notInfectedPersonList.Count > 0)
            {
                closestNotInfectedPerson = notInfectedPersonList[0];
                closestDistance = closestNotInfectedPerson.Position.DistanceFromOtherPoint(p.Position);
            }
            else 
            {
                return;
            }

            double distanceTemp = 0;
            for (int i = 1; i < notInfectedPersonList.Count; i++)
            {
                distanceTemp = notInfectedPersonList[i].Position.DistanceFromOtherPoint(p.Position);
                if (closestDistance > distanceTemp)
                {
                    closestDistance = distanceTemp;
                    closestNotInfectedPerson = notInfectedPersonList[i];
                }
            }

            Point movementDirection = p.Position.GetMovementDirectionToOtherPoint(closestNotInfectedPerson.Position);

            int dx = movementDirection.X;
            int dy = movementDirection.Y;
            Point point = p.Move(dx, dy, MapXMax, MapYMax);

        }

        public void RandomMove(Person p, int boundaryX, int boundaryY)
        {
            int dx = StaticRandom.Rand(-p.Speed, p.Speed + 1);
            int dy = StaticRandom.Rand(-p.Speed, p.Speed + 1);
            p.Move(dx, dy, boundaryX, boundaryY);
                        
            
        }

        public ConcurrentDictionary<int, Person> InitPersonDictionaryRandom(int PeopleCount)
        {
            ConcurrentDictionary<int, Person> personDictionary = new ConcurrentDictionary<int, Person>();

            for (int i = 0; i < PeopleCount; i++)
            {
                Person temp = GeneratePersonInsideBoundaries(0, MapXMax, 0, MapYMax);
                personDictionary.TryAdd(temp.Id, temp);
            }

            return personDictionary;
        }

        public Person GeneratePersonInsideBoundaries(int minX, int maxX, int minY, int maxY)
        {
            Point point = new Point(RandomGenerator.Generate(minX, maxX), RandomGenerator.Generate(minY, maxY));

            return new Person(point);
        }
    }

}
