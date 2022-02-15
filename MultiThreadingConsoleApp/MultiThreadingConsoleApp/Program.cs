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


    class Program
    {
        public static Random random = new Random();
        public static CancellationTokenSource source = new CancellationTokenSource();

        public static  int mapXMax = 0;
        public static int mapYMax = 0;

        public static  int peopleCount = 0;

        public static int infectedCount = 0;

        public static int threadCount = 4 ;

        public static Stopwatch sw = new Stopwatch();

        public static List<Task> threads = new List<Task>();

        public static ConcurrentDictionary<int, Person> globalPersonDictionary;

        public static List<bool> isDoneWIthMovement;

        public static bool isPrinterDone;
        public static Data data;

        public static Map map;

        public static List<bool> infectedPeopleStart;


        public static Task timerThread;

        public static Task printerThread;

        public static Task StatusprinterThread;

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
                PrintMap(globalPersonDictionary);
            }
        }


static void Main(string[] args)
        {
            Console.CursorVisible = false;
            timerThread = new Task(StartTimer, TaskCreationOptions.LongRunning);
            printerThread = new Task(StartPrinter, TaskCreationOptions.LongRunning);
            //StatusprinterThread = new Task(StartPrinter, TaskCreationOptions.LongRunning);

            isDoneWIthMovement = new List<bool>();


            data = new Data();

            data = data.LoadData(FileHandler.ReadFromFile());

            mapXMax = data.mapX;
            mapYMax = data.mapY;

            peopleCount = data.personList.Count;
            infectedCount = data.personList.Where(x => x.IsInfected).ToList().Count;


            map = new Map(mapXMax, mapYMax);

            globalPersonDictionary = InitPersonDictionary(data.personList);


            infectedPeopleStart = new List<bool>();

            foreach (var item in globalPersonDictionary.Values)
            {
                infectedPeopleStart.Add(item.IsInfected);
            }


            List<ConcurrentDictionary<int, Person>> listDictionary = new List<ConcurrentDictionary<int, Person>>();

            Dictionary<int, Person>  personDictionarytemp = new Dictionary<int, Person>();

            int personDistributedCount = 0;


            for (int i = 0; i < threadCount; i++)
            {
                for (int j = 0; j < peopleCount / threadCount; j++)
                {
                    personDictionarytemp.Add(i * peopleCount / threadCount+j, globalPersonDictionary[i * peopleCount / threadCount+j]);
                    personDistributedCount++;

                }
                listDictionary.Add(new ConcurrentDictionary<int, Person>(personDictionarytemp));
                 personDictionarytemp = new Dictionary<int, Person>();
            }

            for (int k = 1; k <= peopleCount - personDistributedCount; k++)
            {

                listDictionary[threadCount - 1].TryAdd(peopleCount - personDistributedCount - k, globalPersonDictionary[peopleCount - personDistributedCount - k]);

            }

            //TODO: START THREAD WITHOUT INCREASEING THE VALUE!!!!!!!!!!!!!!!!!

            foreach (var item in listDictionary)
            {
                isDoneWIthMovement.Add(false);
                threads.Add(new Task(() => MainMethod(item, globalPersonDictionary),TaskCreationOptions.LongRunning));

            }

            foreach (var item in threads)
            {
                item.Start();
            }

            timerThread.Start();
            printerThread.Start();
            //StatusprinterThread.Start();

            Console.ReadLine();
        }


        public static void PrintMap(ConcurrentDictionary<int, Person> globalPersonDictionary) {

            isPrinterDone = false;

            while (globalPersonDictionary.Where(x => x.Value.PreviousPosition == null).ToList().Count > 0)
            {

            }

            printerThread.Wait(10);
            List<int> counts = new List<int>();

            foreach (var item in globalPersonDictionary)
            {
                counts.Add(InfectPerson(globalPersonDictionary, item.Value.Position).Count);
            }
            map.UpdateMap(globalPersonDictionary,counts);
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
                FileHandler.WriteToFile(data.SaveContent(),false);
            }
        }


        public static List<Person> InfectPerson(ConcurrentDictionary<int, Person> globalPersonDictionary, Point pos)
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

        private static void Thinking()
        {
            Thread.Sleep(1);
            /*
            for (int i = 0; i < 50000000; i++){ }
            */
        }

        public static void MainMethod(ConcurrentDictionary<int, Person> personDictionary, ConcurrentDictionary<int, Person> globalPersonDictionary) {
            while (infectedCount != peopleCount)
            {
                isDoneWIthMovement[Task.CurrentId.Value-1] = false;

                MovePeople(personDictionary);

                isDoneWIthMovement[Task.CurrentId.Value - 1] = true;

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


        public static void MovePeople(ConcurrentDictionary<int, Person> personDictionary)
        {
            foreach (var item in personDictionary)
            {
                Thinking();
                if (item.Value.remainingPositions.Count>0)
                {
                    item.Value.Move();
                }
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

    }


}
