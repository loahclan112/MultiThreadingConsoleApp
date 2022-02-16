using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MultiThreadingConsoleApp
{

    class SimulationRandom : ISimluation
    {
        public static Random random = new Random();
        public static CancellationTokenSource source = new CancellationTokenSource();

        public  int mapXMax = 150;
        public  int mapYMax = 75;

        public bool isMapVisible;

        public  int peopleCount = 20;

        public static int infectedCount = 4;

        public int threadCount = 1;

        public static Stopwatch sw = new Stopwatch();

        public static List<Task> threads = new List<Task>();

        public static ConcurrentDictionary<int, Person> globalPersonDictionary;

        public static List<bool> isDoneWIthMovement;

        public static bool isPrinterDone;

        public bool isZombieModeOn;

        public static Data data;

        public static List<bool> infectedPeopleStart;
        public static Map map;

        public static Task timerThread;

        public static Task printerThread;
        public static Task statusprinterThread;



        public SimulationRandom(int threadCount = 1, int mapXMax = 150, int mapYMax = 75, int peopleCount = 20, bool isMapVisible = true, bool isZombieModeOn = true )
        {
            this.threadCount = threadCount;
            this.isMapVisible = isMapVisible;
            this.mapXMax = mapXMax;
            this.mapYMax = mapYMax;
            this.peopleCount = peopleCount;
            this.isZombieModeOn = isZombieModeOn;
        }

        public void StartTimer() {


            sw.Start();
            int CursorPos = isMapVisible ? mapYMax : 0;

            while (!source.IsCancellationRequested)
            {
                lock (MyLocks.lockConsoleObject)
                {
                    Console.SetCursorPosition(0, CursorPos + 3);
                    Console.WriteLine("Elapsed Seconds: " + (double)sw.ElapsedMilliseconds / 1000 + " s");
                }
            }
        }

        public void StartPrinter()
        {
            while (!source.IsCancellationRequested)
            {
                PrintMap(globalPersonDictionary,isMapVisible);
            }
        }


        public void StartSimulation()
        {

            Console.CursorVisible = false;
            timerThread = new Task(StartTimer, TaskCreationOptions.LongRunning);
            printerThread = new Task(StartPrinter, TaskCreationOptions.LongRunning);

            isDoneWIthMovement = new List<bool>();

            map = new Map(mapXMax, mapYMax);

            data = new Data();
            data.mapX = mapXMax;
            data.mapY = mapYMax;


            globalPersonDictionary = InitPersonDictionaryRandom(peopleCount);

            InfectRandomPeople(globalPersonDictionary, infectedCount);

            infectedPeopleStart = new List<bool>();

            foreach (var item in globalPersonDictionary.Values)
            {
                infectedPeopleStart.Add(item.IsInfected);
            }

            List<ConcurrentDictionary<int, Person>> listDictionary = new List<ConcurrentDictionary<int, Person>>();

            Dictionary<int, Person> personDictionarytemp = new Dictionary<int, Person>();

            int personDistributedCount = 0;

            for (int i = 0; i < threadCount; i++)
            {
                for (int j = 0; j < peopleCount / threadCount; j++)
                {
                    personDictionarytemp.Add(i * (peopleCount / threadCount) + j, globalPersonDictionary[i * (peopleCount / threadCount) + j]);
                    personDistributedCount++;
                }

                listDictionary.Add(new ConcurrentDictionary<int, Person>(personDictionarytemp));
                personDictionarytemp = new Dictionary<int, Person>();
            }

            for (int k = 1; k <= peopleCount - personDistributedCount; k++)
            {

                listDictionary[threadCount - 1].TryAdd(peopleCount - k, globalPersonDictionary[peopleCount - k]);

            }

            foreach (var item in listDictionary)
            {
                isDoneWIthMovement.Add(false);
                if (isZombieModeOn)
                {
                    threads.Add(new Task(() => ThreadMethodZombie(item, globalPersonDictionary), TaskCreationOptions.LongRunning));

                }
                else {
                    threads.Add(new Task(() => ThreadMethod(item), TaskCreationOptions.LongRunning));

                }

            }

            foreach (var item in threads)
            {
                item.Start();
            }

            timerThread.Start();
            printerThread.Start();
            Console.ReadLine();
        }



        public void PrintMap(ConcurrentDictionary<int, Person> globalPersonDictionary, bool isMapVisible) {

            isPrinterDone = false;

            int CursorPos = isMapVisible ? mapYMax : 0;


            while (globalPersonDictionary.Where(x => x.Value.PreviousPosition == null).ToList().Count>0) {
            
            }

            bool isEveryThreadDoneWithMovements = false;

            while (!isEveryThreadDoneWithMovements)
            {
                isEveryThreadDoneWithMovements = true;

                for (int i = 0; i < isDoneWIthMovement.Count; i++)
                {
                    if (!isDoneWIthMovement[i])
                    {
                        isEveryThreadDoneWithMovements = false;
                        break;
                    }
                }
            }

            List<int> counts = new List<int>();

            foreach (var item in globalPersonDictionary)
            {
                counts.Add(InfectPerson(globalPersonDictionary, item.Value.Position).Count);
            }

            int temp = getInfectedPeopleCount(globalPersonDictionary);
            lock (MyLocks.lockInfectedCountObject)
            {
                infectedCount = temp;
            }
            if (isMapVisible)
            {
                map.UpdateMap(globalPersonDictionary, counts);
            }
            lock (MyLocks.lockConsoleObject)
            {
                Console.SetCursorPosition(0, CursorPos + 1);
                Console.WriteLine("Thread Count: " + threadCount);

                Console.SetCursorPosition(0, CursorPos + 2);
                Console.WriteLine("Infected People Count: " + infectedCount + "/ " + peopleCount + "  ( " + ((double)infectedCount / peopleCount) * 100 + " % )");
            }

            isPrinterDone = true;

            if (source.IsCancellationRequested)
            {
                data.personList = globalPersonDictionary.Values.ToList();
                for (int i = 0; i < infectedPeopleStart.Count; i++)
                {
                    data.personList[i].IsInfected = infectedPeopleStart[i];
                }
                FileHandler.WriteToFile(data.SaveContent(),true);
            }
        }

        public List<Person> InfectPerson(ConcurrentDictionary<int, Person> globalPersonDictionary, Point pos)
        {
            List<Person> infectedPersonList = globalPersonDictionary.Values.Where(x => Math.Abs(x.Position.X - pos.X) < 3 && Math.Abs(x.Position.Y - pos.Y) < 1).ToList();
            if (infectedPersonList.Count <= 1)
            {
                return infectedPersonList;
            }

            foreach (Person item in infectedPersonList)
            {
                if (item.IsInfected)
                {
                    foreach (Person victim in infectedPersonList)
                    {
                        victim.IsInfected = true;
                    }
                    break;
                }
            }
            return infectedPersonList;
        }

        public void ThreadMethod(ConcurrentDictionary<int, Person> personDictionary) {
            while (infectedCount != peopleCount)
            {
                isDoneWIthMovement[Task.CurrentId.Value - 1] = false;

                MovePeople(personDictionary);
                isDoneWIthMovement[Task.CurrentId.Value - 1] = true;

                while (!isPrinterDone)
                {

                }

            }
            source.Cancel();
        }

        public void ThreadMethodZombie(ConcurrentDictionary<int, Person> personDictionary, ConcurrentDictionary<int, Person> globalPersonDictionary)
        {
            while (infectedCount != peopleCount)
            {
                isDoneWIthMovement[Task.CurrentId.Value - 1] = false;

                MovePeopleZombie(personDictionary, globalPersonDictionary);
                isDoneWIthMovement[Task.CurrentId.Value - 1] = true;

                while (!isPrinterDone)
                {

                }
            }
            source.Cancel();
        }

        public int getInfectedPeopleCount(ConcurrentDictionary<int, Person> personDictionary)
        {
            return personDictionary.Where(x => x.Value.IsInfected).ToList().Count;
        }

        public void InfectOnePerson(ConcurrentDictionary<int, Person> personDictionary, int id)
        {
            personDictionary[id].IsInfected = true;
        }

        public void InfectRandomPeople(ConcurrentDictionary<int, Person> personDictionary, int count)
        {
            List<int> alreadyInfected = new List<int>();

            count = Math.Min(count, peopleCount);

            int id;

            while (alreadyInfected.Count != count)
            {
                id = random.Next(count);
                if (!alreadyInfected.Contains(id))
                {
                    personDictionary[id].IsInfected = true;
                    alreadyInfected.Add(id);
                }
            }

        }

        public void MovePeopleZombie(ConcurrentDictionary<int, Person> personDictionary, ConcurrentDictionary<int, Person> globalpersonDictionary )
        {
            foreach (var item in personDictionary)
            {
                Thinking();
                if (!item.Value.IsInfected)
                {
                    RandomMove(item.Value, mapXMax, mapYMax);
                }
                else 
                {
                    ZombieMove(item.Value, globalpersonDictionary, mapXMax, mapYMax);
                }
                item.Value.DonePositions.Add(item.Value.Position);
            }

        }

        public void Thinking()
        {
            Thread.Sleep(1);
        }

        public void MovePeople(ConcurrentDictionary<int, Person> personDictionary)
        {
            foreach (var item in personDictionary)
            {
                Thinking();
                RandomMove(item.Value, mapXMax, mapYMax);
                item.Value.DonePositions.Add(item.Value.Position);

            }

        }

        public void ZombieMove(Person p, ConcurrentDictionary<int, Person> globalpersonDictionary, int mapXMax, int mapYMax)
        {
            Person closestNotInfectedPerson;

            double closestDistance = double.MaxValue;

            List<Person> notInfectedPersonList = globalpersonDictionary.Values.Where(x => !x.IsInfected).ToList();

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
            Point point = p.Move(dx, dy, mapXMax, mapYMax);

        }


        /*
        public static void MovePeopleRandom(ConcurrentDictionary<int, Person> personDictionary)
        {
            foreach (var item in personDictionary)
            {
                RandomMove(item.Value, mapXMax, mapYMax);
                item.Value.donePositions.Add(item.Value.Position);
            }

        }
        */

        public void RandomMove(Person p, int boundaryX, int boundaryY)
        {
            int dx = random.Next(-p.Speed, p.Speed + 1);
            int dy = random.Next(-p.Speed, p.Speed + 1);
            Point point = p.Move(dx, dy, boundaryX, boundaryY);
            
            while (point == null)
            {
                dx = random.Next(-p.Speed, p.Speed + 1);
                dy = random.Next(-p.Speed, p.Speed + 1);
                point = p.Move(dx, dy, boundaryX, boundaryY);
            }
            
        }

        public ConcurrentDictionary<int, Person> InitPersonDictionary(List<Person> personList)
        {
            ConcurrentDictionary<int, Person> personDictionary = new ConcurrentDictionary<int, Person>();

            foreach (var item in personList)
            {
                personDictionary.TryAdd(item.Id, item);

            }
        
            return personDictionary;
        }


        public ConcurrentDictionary<int, Person> InitPersonDictionaryRandom(int peopleCount)
        {
            ConcurrentDictionary<int, Person> personDictionary = new ConcurrentDictionary<int, Person>();

            for (int i = 0; i < peopleCount; i++)
            {
                Person temp = GeneratePersonInsideBoundaries(0, mapXMax, 0, mapYMax);
                personDictionary.TryAdd(temp.Id, temp);
            }

            return personDictionary;
        }

        public Person GeneratePersonInsideBoundaries(int minX, int maxX, int minY, int maxY)
        {
            Point point = new Point(random.Next(minX, maxX), random.Next(minY, maxY));

            return new Person(point);
        }

    }


}
