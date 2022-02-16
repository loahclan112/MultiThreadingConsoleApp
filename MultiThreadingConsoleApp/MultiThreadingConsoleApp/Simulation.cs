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
     public class Simulation : ISimluation
    {
        public CancellationTokenSource source = new CancellationTokenSource();

        public  int mapXMax = 0;
        public int mapYMax = 0;

        public  int peopleCount = 0;

        public int infectedCount = 0;

        public int threadCount = 3;

        public Stopwatch sw = new Stopwatch();

        public List<Task> threads = new List<Task>();

        public ConcurrentDictionary<int, Person> globalPersonDictionary;

        public bool isPrinterDone;
        public Data data;

        public Map map;

        public List<bool> infectedPeopleStart;

        public Task timerThread;

        public Task printerThread;

        public bool isMapVisible;

        public static List<bool> isDoneWIthMovement;


        public Simulation(int threadCount = 1, bool isMapVisible=true) 
        {
            this.threadCount = threadCount;
            this.isMapVisible = isMapVisible;
        
        }

        public void StartTimer() 
        {
            sw.Start();
            int CursorPos = isMapVisible ? mapYMax : 0;

            while (!source.IsCancellationRequested) 
            {
                lock (MyLocks.lockConsoleObject)
                {
                    Console.SetCursorPosition(0, CursorPos + 3);
                    Console.WriteLine("Elapsed Seconds: " + (double)sw.ElapsedMilliseconds / 1000 +" s");
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

        public void StartSimulation() {
            Console.CursorVisible = false;
            timerThread = new Task(StartTimer, TaskCreationOptions.LongRunning);
            printerThread = new Task(StartPrinter, TaskCreationOptions.LongRunning);
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
                threads.Add(new Task(() => ThreadMethod(item, globalPersonDictionary), TaskCreationOptions.LongRunning));
            }

            foreach (var item in threads)
            {
                item.Start();
            }

            timerThread.Start();
            printerThread.Start();        
        }


        public void PrintMap(ConcurrentDictionary<int, Person> globalPersonDictionary, bool isMapVisible) {

            isPrinterDone = false;

            int CursorPos = isMapVisible ? mapYMax : 0;

            while (globalPersonDictionary.Where(x => x.Value.PreviousPosition == null).ToList().Count > 0)
            {

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
            if (isMapVisible)
            {
                map.UpdateMap(globalPersonDictionary, counts);
            }


            int temp = getInfectedPeopleCount(globalPersonDictionary);
            lock (MyLocks.lockInfectedCountObject)
            {
                infectedCount = temp;
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
                FileHandler.WriteToFile(data.SaveContent(),false);
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

        public void Thinking()
        {
            Thread.Sleep(1);
            /*
            for (int i = 0; i < 50000000; i++){ }
            */
        }

        public void ThreadMethod(ConcurrentDictionary<int, Person> personDictionary, ConcurrentDictionary<int, Person> globalPersonDictionary) {
            while (infectedCount < peopleCount)
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


        public int getInfectedPeopleCount(ConcurrentDictionary<int, Person> personDictionary)
        {

            return personDictionary.Where(x => x.Value.IsInfected).ToList().Count;
        }


        public void MovePeople(ConcurrentDictionary<int, Person> personDictionary)
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

        public ConcurrentDictionary<int, Person> InitPersonDictionary(List<Person> personList)
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
