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
    public abstract class SimulationBase : ISimulation
    {
        private bool isDone;
        private bool isPrinterDone;
        private List<bool> isDoneWithMovement;
        private int currentIteartion = 0;
        private int infectedCount = 4;
        private int peopleCount = 20;
        private List<Task> threads = new List<Task>();
        private Map map;
        private int threadCount = 1;
        private CancellationTokenSource source = new CancellationTokenSource();
        private ConcurrentDictionary<int, Person> globalPersonDictionary;
        private bool isMapVisible;
        private int mapYMax = 0;
        private Stopwatch sw = new Stopwatch();
        private int mapXMax = 0;

        public bool IsDone { get => isDone; set => isDone = value; }
        public bool IsPrinterDone { get => isPrinterDone; set => isPrinterDone = value; }
        public List<bool> IsDoneWithMovement { get => isDoneWithMovement; set => isDoneWithMovement = value; }
        public int CurrentIteartion { get => currentIteartion; set => currentIteartion = value; }
        public int InfectedCount { get => infectedCount; set => infectedCount = value; }
        public int PeopleCount { get => peopleCount; set => peopleCount = value; }
        public List<Task> Threads { get => threads; set => threads = value; }
        public Map Map { get => map; set => map = value; }
        public int ThreadCount { get => threadCount; set => threadCount = value; }
        public CancellationTokenSource Source { get => source; set => source = value; }
        public ConcurrentDictionary<int, Person> GlobalPersonDictionary { get => globalPersonDictionary; set => globalPersonDictionary = value; }
        public bool IsMapVisible { get => isMapVisible; set => isMapVisible = value; }
        public int MapYMax { get => mapYMax; set => mapYMax = value; }
        public Stopwatch Sw { get => sw; set => sw = value; }
        public int MapXMax { get => mapXMax; set => mapXMax = value; }

        public int getInfectedPeopleCount(ConcurrentDictionary<int, Person> personDictionary)
        {
            return personDictionary.Where(x => x.Value.Status == StatusEnum.Infected).ToList().Count;
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
                if (item.Status == StatusEnum.Infected)
                {
                    foreach (Person victim in infectedPersonList)
                    {
                        victim.Status = StatusEnum.Infected;
                    }
                    break;
                }
            }
            return infectedPersonList;
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

        public bool CheckIsDone()
        {
            //Person.commonId = 0;
            return IsDone;
        }

        public abstract void MovePeople(ConcurrentDictionary<int, Person> personDictionary);

        public void StartPrinter()
        {
            while (!Source.IsCancellationRequested)
            {
                Synchronize(GlobalPersonDictionary, IsMapVisible);
            }
            Source.Cancel();
        }

        public abstract void StartSimulation();

        public void StartTimer()
        {
            Sw.Start();
            int CursorPos = IsMapVisible ? MapYMax : 0;

            while (!Source.IsCancellationRequested)
            {
                lock (MyLocks.lockConsoleObject)
                {
                    Console.SetCursorPosition(0, CursorPos + 4);
                    Console.WriteLine("Elapsed Seconds: " + (double)Sw.ElapsedMilliseconds / 1000 + " s");
                }
            }
            Source.Cancel();
        }

        public virtual void Synchronize(ConcurrentDictionary<int, Person> globalPersonDictionary, bool isMapVisible)
        {

            IsPrinterDone = false;
            int CursorPos = isMapVisible ? MapYMax : 0;

            while (globalPersonDictionary.Where(x => x.Value.PreviousPosition == null).ToList().Count > 0)
            {

            }

            bool isEveryThreadDoneWithMovements = false;

            while (!isEveryThreadDoneWithMovements)
            {
                isEveryThreadDoneWithMovements = true;

                for (int i = 0; i < IsDoneWithMovement.Count; i++)
                {
                    if (!IsDoneWithMovement[i])
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
                InfectedCount = temp;
               CurrentIteartion += 1;
            }
            if (isMapVisible)
            {
                Map.UpdateMap(globalPersonDictionary, counts);
            }
            lock (MyLocks.lockConsoleObject)
            {
                Console.SetCursorPosition(0, CursorPos + 1);
                Console.WriteLine("Thread Count: " + ThreadCount);

                Console.SetCursorPosition(0, CursorPos + 2);
                Console.WriteLine("Iteration: " + CurrentIteartion);

                Console.SetCursorPosition(0, CursorPos + 3);
                Console.WriteLine("Infected People Count: " + InfectedCount + "/ " + PeopleCount + "  ( " + ((double)InfectedCount / PeopleCount) * 100 + " % )");
            }

            IsPrinterDone = true;
            EndCheck();
        }




        public abstract void Thinking();

        public virtual void ThreadMethod(ConcurrentDictionary<int, Person> personDictionary)
        {
            int index = Threads.FindIndex(x => x.Id == Task.CurrentId);
            while (InfectedCount < PeopleCount)
            {
                lock (MyLocks.lockIsDoneMovementObject)
                {
                    IsDoneWithMovement[index] = false;
                }

                MovePeople(personDictionary);

                lock (MyLocks.lockIsDoneMovementObject)
                {
                    IsDoneWithMovement[index] = true;
                }
            

                while (IsDoneWithMovement[index] && !IsPrinterDone)
                {

                }
            }
           Source.Cancel();
           Task.WaitAll(Threads.ToArray());
           Threads[index].Dispose();

        }

        public abstract void EndCheck();
    }
}
