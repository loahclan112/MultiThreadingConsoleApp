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

        public static int threadCount = 1 ;

        public static Stopwatch sw = new Stopwatch();

        public static List<Task> threads = new List<Task>();

        public static ConcurrentDictionary<int, Person> personDictionary;

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
                PrintMap(personDictionary);
            }
        }

        public static void StartPrinterasd(ConcurrentDictionary<int, Person>  globalPersonDictionary)
        {

            while (!source.IsCancellationRequested)
            {
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
            }
        }



static void Main2(string[] args)
        {
            Console.CursorVisible = false;
            timerThread = new Task(StartTimer, TaskCreationOptions.LongRunning);
            printerThread = new Task(StartPrinter, TaskCreationOptions.LongRunning);
            StatusprinterThread = new Task(StartPrinter, TaskCreationOptions.LongRunning);

            isDoneWIthMovement = new List<bool>();


            data = new Data();

            data = data.LoadData(FileHandler.ReadFromFile());

            mapXMax = data.mapX;
            mapYMax = data.mapY;

            peopleCount = data.personList.Count;
            infectedCount = data.personList.Where(x => x.IsInfected).ToList().Count;


            map = new Map(mapXMax, mapYMax);

             personDictionary = InitPersonDictionary(data.personList);


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
            StatusprinterThread.Start();

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

            printerThread.Wait(50);
            map.UpdateMap(globalPersonDictionary);


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

                MovePeople(personDictionary);

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


        public static void MovePeople(ConcurrentDictionary<int, Person> personDictionary)
        {
            foreach (var item in personDictionary)
            {
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
