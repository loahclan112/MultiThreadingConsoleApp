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

    class ProgramRandom
    {
        public static Random random = new Random();
        public static CancellationTokenSource source = new CancellationTokenSource();

        public const int mapXMax = 100;
        public const int mapYMax = 50;

        public const int peopleCount = 1000;

        public static int infectedCount = 50;

        public static int threadCount = 1;

        public static Stopwatch sw = new Stopwatch();

        public static List<Task> threads = new List<Task>();

        public static ConcurrentDictionary<int, Person> personDictionary;

        public static List<bool> isDoneWIthMovement;

        public static bool isPrinterDone;

        public static Data data;

        public static List<bool> infectedPeopleStart;
        public static Map map;

        public static Task timerThread;

        public static Task printerThread;

        public static void StartTimer() {

          
            sw.Start();
            while (!source.IsCancellationRequested) {
                Task.Delay(0, source.Token);
                lock (MyLocks.lockConsoleObject)
                {
                    Console.SetCursorPosition(0, mapYMax + 2);
                    Console.WriteLine("Elapsed Seconds: " + (double)sw.ElapsedMilliseconds / 1000 +" s");
                }
            }
        }

        public static void StartPrinter()
        {

            while (!source.IsCancellationRequested)
            {
                Task.Delay(0, source.Token);
                PrintMap(personDictionary);
            }
        }

        static void Main2(string[] args)
        {
            Console.CursorVisible = false;
            timerThread = new Task(StartTimer, TaskCreationOptions.LongRunning);
            printerThread = new Task(StartPrinter, TaskCreationOptions.LongRunning);

            isDoneWIthMovement = new List<bool>();

          
            map = new Map(mapXMax, mapYMax);

            data = new Data();
            data.mapX = mapXMax;
            data.mapY = mapYMax;


            personDictionary = InitPersonDictionaryRandom(peopleCount);
             //personDictionary = InitPersonDictionary(peopleCount);




            InfectRandomPeople(personDictionary, infectedCount);

            infectedPeopleStart = new List<bool>();

            foreach (var item in personDictionary.Values)
            {
                infectedPeopleStart.Add(item.IsInfected);
            }

            List<ConcurrentDictionary<int, Person>> listDictionary = new List<ConcurrentDictionary<int, Person>>();

            Dictionary<int, Person>  personDictionarytemp = new Dictionary<int, Person>();

            for (int i = 0; i < threadCount; i++)
            {
                for (int j = 0; j < peopleCount / threadCount; j++)
                {
                    personDictionarytemp.Add(i * peopleCount / threadCount+j, personDictionary[i * peopleCount / threadCount+j]);
                }
                listDictionary.Add(new ConcurrentDictionary<int, Person>(personDictionarytemp));
                 personDictionarytemp = new Dictionary<int, Person>();
            }

            //TODO: START THREAD WITHOUT INCREASEING THE VALUE!!!!!!!!!!!!!!!!!

            foreach (var item in listDictionary)
            {
                isDoneWIthMovement.Add(false);
                threads.Add(new Task(() => MainMethod(item, personDictionary),TaskCreationOptions.LongRunning));

            }

            foreach (var item in threads)
            {
                item.Start();
            }

            timerThread.Start();
            printerThread.Start();

            Console.ReadLine();
        }


        public static void PrintMap(ConcurrentDictionary<int, Person> globalPersonDictionary) {

            bool isEveryThreadDoneWithMovements = false;
            isPrinterDone = false;

            while (!isEveryThreadDoneWithMovements) {

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

            map.UpdateMap(globalPersonDictionary);
            int temp = getInfectedPeopleCount(globalPersonDictionary);
            lock (MyLocks.lockInfectedCountObject)
            {
                infectedCount = temp;
            }
            lock (MyLocks.lockConsoleObject)
            {
                Console.SetCursorPosition(0, mapYMax + 1);
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
                FileHandler.WriteToFile(data.SaveContent());
            }
        }



        public static void MainMethod(ConcurrentDictionary<int, Person> personDictionary, ConcurrentDictionary<int, Person> globalPersonDictionary) {
            while (infectedCount != peopleCount)
            {
                isDoneWIthMovement[Task.CurrentId.Value-1] = false;

                MovePeopleRandom(personDictionary);

                isDoneWIthMovement[Task.CurrentId.Value - 1] = true;

                while (!isPrinterDone) {
                
                }
            }

            source.Cancel();
            
        }


        public static int getInfectedPeopleCount(ConcurrentDictionary<int, Person> personDictionary)
        {

            return personDictionary.Where(x => x.Value.IsInfected).ToList().Count;
        }

        public static void InfectOnePerson(ConcurrentDictionary<int, Person> personDictionary, int id)
        {
            personDictionary[id].IsInfected = true;
        }

        public static void InfectRandomPeople(ConcurrentDictionary<int, Person> personDictionary, int count)
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

        public static void MovePeopleRandom(ConcurrentDictionary<int, Person> personDictionary)
        {
            foreach (var item in personDictionary)
            {
                RandomMove(item.Value, mapXMax, mapYMax);
                item.Value.donePositions.Add(item.Value.Position);
            }

        }

        public static void RandomMove(Person p, int boundaryX, int boundaryY)
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

        public static ConcurrentDictionary<int, Person> InitPersonDictionary(List<Person> personList)
        {
            ConcurrentDictionary<int, Person> personDictionary = new ConcurrentDictionary<int, Person>();

            foreach (var item in personList)
            {
                personDictionary.TryAdd(item.Id, item);

            }
        
            return personDictionary;
        }


        public static ConcurrentDictionary<int, Person> InitPersonDictionaryRandom(int peopleCount)
        {
            ConcurrentDictionary<int, Person> personDictionary = new ConcurrentDictionary<int, Person>();

            for (int i = 0; i < peopleCount; i++)
            {
                Person temp = GeneratePersonInsideBoundaries(0, mapXMax, 0, mapYMax);
                personDictionary.TryAdd(temp.Id, temp);
            }

            return personDictionary;
        }

        public static Person GeneratePersonInsideBoundaries(int minX, int maxX, int minY, int maxY)
        {
            Point point = new Point(random.Next(minX, maxX), random.Next(minY, maxY));

            return new Person(point);
        }

    }


}
