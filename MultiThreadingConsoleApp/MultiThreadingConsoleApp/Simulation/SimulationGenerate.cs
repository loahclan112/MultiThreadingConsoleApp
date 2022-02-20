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
        public bool isZombieModeOn;

        public SimulationGenerate(int threadCount = 1, int MapXMax = 150, int MapYMax = 75, int PeopleCount = 20, bool isMapVisible = true, bool isZombieModeOn = true, int InfectedCount = 1, double infectionRate = 1, double recoveryRate = 3 )
        {
            this.ThreadCount = threadCount;
            this.IsMapVisible = isMapVisible;
            this.MapXMax = MapXMax;
            this.MapYMax = MapYMax;
            this.PeopleCount = PeopleCount;
            this.isZombieModeOn = isZombieModeOn;
            this.InfectedCount = InfectedCount;
            this.InfectionRate = infectionRate;
            this.RecoveryRate = recoveryRate;
        }

        public override void StartSimulation()
        {
          
            Console.CursorVisible = false;
            TimerThread = new Task(StartTimer, Source.Token, TaskCreationOptions.LongRunning);
            PrinterThread = new Task(StartPrinter, Source.Token, TaskCreationOptions.LongRunning);

            IsDoneWithMovement = new List<bool>();

            Map = new Map(MapXMax, MapYMax);

            Data = new Data();
            Data.MapX = MapXMax;
            Data.MapY = MapYMax;
            Data.RecoveryRate = RecoveryRate;
            Data.InfectionRate = InfectionRate;


            GlobalPersonDictionary = InitPersonDictionaryRandom(PeopleCount,RecoveryRate);

            InfectRandomPeople(GlobalPersonDictionary, InfectedCount);

            InfectedPeopleStart = new List<StatusEnum>();

            foreach (var item in GlobalPersonDictionary.Values)
            {
                InfectedPeopleStart.Add(item.Status);
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

            foreach (var item in listDictionary)
            {
                IsDoneWithMovement.Add(false);
                if (isZombieModeOn)
                {
                    Threads.Add(new Task(() => ThreadMethodZombie(item, GlobalPersonDictionary), TaskCreationOptions.LongRunning));

                }
                else {
                    Threads.Add(new Task(() => ThreadMethod(item), TaskCreationOptions.LongRunning));

                }
            }

            foreach (var item in Threads)
            {
                item.Start();
            }

            TimerThread.Start();
            PrinterThread.Start();
        }

        public override void EndCheck()
        {
            if (InfectedCount >= PeopleCount || InfectedCount <= 0)
            {
                Source.Cancel();

            }
            if (Source.IsCancellationRequested)
            {
                Data.PersonList = GlobalPersonDictionary.Values.ToList();
                for (int i = 0; i < InfectedPeopleStart.Count; i++)
                {
                    Data.PersonList[i].Status = InfectedPeopleStart[i];
                }
                FileHandler.WriteToFile(Data.SaveContent(), true);
                FileHandler.WriteToFileExcel(Data.SaveCountContent());

                Person.commonId = 0;
                IsDone = true;
                List<Task> remainingTasks = new List<Task>();

                remainingTasks.Add(TimerThread);
                remainingTasks.Add(PrinterThread);
                Task.WaitAll(remainingTasks.ToArray());
                for (int i = 0; i < remainingTasks.Count; i++)
                {
                    remainingTasks[i].Dispose();
                }
            }
        }

        public void ThreadMethodZombie(ConcurrentDictionary<int, Person> personDictionary, ConcurrentDictionary<int, Person> GlobalPersonDictionary)
        {
            int index = Threads.FindIndex(x => x.Id == Task.CurrentId);
            while (InfectedCount < PeopleCount && InfectedCount > 0)
            {
                IsDoneWithMovement[index] = false;
                PeopleStatusUpdate(personDictionary);
                MovePeopleZombie(personDictionary, GlobalPersonDictionary);
                IsDoneWithMovement[index] = true;

                while (!IsPrinterDone)
                {

                }
            }
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
            foreach (var item in personDictionary.Values)
            {
                CalculateInfection(personDictionary,item);
                if (item.Status != StatusEnum.Infected)
                {
                    RandomMove(item, MapXMax, MapYMax);
                }
                else 
                {
                    ZombieMove(item, GlobalPersonDictionary, MapXMax, MapYMax);
                }
                item.DonePositions.Add(item.Position);
            }
        }

        public override void CalculateInfection(ConcurrentDictionary<int, Person> personDictionary,Person person)
        {
            if (IsMapVisible)
            {
                for (int i = 0; i < 100; i++)
                {
                    person.InfectionRate = (double)StaticRandom.Rand(5, 11) / 10;
                }
            }
            else
            {
                for (int i = 0; i < 10000; i++)
                {
                    person.InfectionRate = (double)StaticRandom.Rand(5, 11) / 10;
                }
            }
        }

        public override void MovePeople(ConcurrentDictionary<int, Person> personDictionary)
        {
            foreach (var item in personDictionary.Values)
            {
                CalculateInfection(personDictionary,item);
                RandomMove(item, MapXMax, MapYMax);
                item.DonePositions.Add(item.Position);
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
            int dx = 0;
            int dy = 0;
            Point point = null;

            do
            {
                dx = StaticRandom.Rand(-p.Speed, p.Speed + 1);
                dy = StaticRandom.Rand(-p.Speed, p.Speed + 1);
                point = p.Move(dx, dy, boundaryX, boundaryY);

            } while (point == null);
            
        }

        public ConcurrentDictionary<int, Person> InitPersonDictionaryRandom(int PeopleCount, double recoveryRate)
        {
            ConcurrentDictionary<int, Person> personDictionary = new ConcurrentDictionary<int, Person>();

            for (int i = 0; i < PeopleCount; i++)
            {
                Person temp = GeneratePersonInsideBoundaries(0, MapXMax, 0, MapYMax);
                temp.RecoveryRate = recoveryRate;
                personDictionary.TryAdd(temp.Id, temp);
            }

            return personDictionary;
        }

        public Person GeneratePersonInsideBoundaries(int minX, int maxX, int minY, int maxY)
        {
            Point point = new Point(RandomGenerator.Generate(minX, maxX), RandomGenerator.Generate(minY, maxY));

            return new Person(point);
        }

        public override bool ThreadMethodExitCondition(ConcurrentDictionary<int, Person> personDictionary)
        {
            return Source.IsCancellationRequested;
        }
    }

}
