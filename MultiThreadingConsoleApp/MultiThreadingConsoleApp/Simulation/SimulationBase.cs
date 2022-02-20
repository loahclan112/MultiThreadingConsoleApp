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
        private int currentIteration = 0;
        private int infectedCount = 4;
        private int susceptibleCount = 0;
        private int recoveredCount = 0;

        private int peopleCount = 20;
        private List<Task> threads = new List<Task>();
        private Task timerThread;
        private Task printerThread;
        private Map map;
        private int threadCount = 1;
        private CancellationTokenSource source = new CancellationTokenSource();
        private ConcurrentDictionary<int, Person> globalPersonDictionary;
        private bool isMapVisible;
        private int mapYMax = 0;
        private Stopwatch sw = new Stopwatch();
        private int mapXMax = 0;
        private int infectionRange = 1;
        private double recoveryRate = 0;
        private double infectionRate = 1;
        private bool isEnd = false;
        private Data data;
        private List<StatusEnum> infectedPeopleStart;
        private List<Dictionary<int, Person>> listDictionary = new List<Dictionary<int, Person>>();


        public bool IsDone { get => isDone; set => isDone = value; }
        public bool IsPrinterDone { get => isPrinterDone; set => isPrinterDone = value; }
        public List<bool> IsDoneWithMovement { get => isDoneWithMovement; set => isDoneWithMovement = value; }
        public int CurrentIteration { get => currentIteration; set => currentIteration = value; }
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
        public double RecoveryRate { get => recoveryRate; set => recoveryRate = value; }
        public int InfectionRange { get => infectionRange; set => infectionRange = value; }
        public double InfectionRate { get => infectionRate; set => infectionRate = value; }
        public bool IsEnd { get => isEnd; set => isEnd = value; }
        public int SusceptibleCount { get => susceptibleCount; set => susceptibleCount = value; }
        public int RecoveredCount { get => recoveredCount; set => recoveredCount = value; }
        public Task TimerThread { get => timerThread; set => timerThread = value; }
        public Task PrinterThread { get => printerThread; set => printerThread = value; }
        public Data Data { get => data; set => data = value; }
        public List<StatusEnum> InfectedPeopleStart { get => infectedPeopleStart; set => infectedPeopleStart = value; }
        public List<Dictionary<int, Person>> ListDictionary { get => listDictionary; set => listDictionary = value; }

        public int getInfectedPeopleCount(ConcurrentDictionary<int, Person> personDictionary)
        {
            return personDictionary.Where(x => x.Value.Status == StatusEnum.Infected).ToList().Count;
        }

        public int getSusceptiblePeopleCount(ConcurrentDictionary<int, Person> personDictionary)
        {
            return personDictionary.Where(x => x.Value.Status == StatusEnum.Susceptible).ToList().Count;
        }

        public int getRecoveredPeopleCount(ConcurrentDictionary<int, Person> personDictionary)
        {
            return personDictionary.Where(x => x.Value.Status == StatusEnum.Recovered).ToList().Count;
        }

        public List<Person> InfectPerson(Point pos)
        {
            List<Person> infectedPersonList = GlobalPersonDictionary.Values.Where(x => Math.Abs(x.Position.X - pos.X) < 3 + InfectionRange && Math.Abs(x.Position.Y - pos.Y) < 1 + InfectionRange).ToList();
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
                        if (victim.InfectionRate >= 0.5)
                        {
                            victim.Status = StatusEnum.Infected;
                        }
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
                item.RecoveryRate = recoveryRate;
                bool asd = personDictionary.TryAdd(item.Id, item);
            }

            return personDictionary;
        }

        public bool CheckIsDone()
        {
            return IsDone;
        }

        public abstract void MovePeople(ConcurrentDictionary<int, Person> personDictionary);

        public void StartPrinter()
        {
            while (!Source.IsCancellationRequested)
            {
                Synchronize(IsMapVisible);
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
                    Console.SetCursorPosition(0, CursorPos + 6);
                    Console.WriteLine("Elapsed Seconds: " + (double)Sw.ElapsedMilliseconds / 1000 + " s");
                }
            }
            Source.Cancel();
        }

        public virtual void Synchronize(bool isMapVisible)
        {


            
            IsPrinterDone = false;
            int CursorPos = isMapVisible ? MapYMax : 0;

            while (GlobalPersonDictionary.Where(x => x.Value.PreviousPosition == null).ToList().Count > 0)
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

            foreach (var item in GlobalPersonDictionary)
            {
                counts.Add(InfectPerson(item.Value.Position).Count);
            }

            lock (MyLocks.lockMainObject)
            {
                int tempInfectedCount = getInfectedPeopleCount(GlobalPersonDictionary);
                int tempSusceptibleCount = getSusceptiblePeopleCount(GlobalPersonDictionary);
                int tempRecoveredCount = getRecoveredPeopleCount(GlobalPersonDictionary);

                InfectedCount = tempInfectedCount;
                SusceptibleCount = tempSusceptibleCount;
                RecoveredCount = tempRecoveredCount;

                CurrentIteration++ ;
                Data.SusceptibleCount.Add(SusceptibleCount);
                Data.InfectedCount.Add(InfectedCount);
                Data.RecoveredCount.Add(RecoveredCount);
            }


            if (isMapVisible)
            {
                Map.UpdateMap(GlobalPersonDictionary, counts);
            }
            lock (MyLocks.lockConsoleObject)
            {
                Console.SetCursorPosition(0, CursorPos + 1);
                Console.WriteLine("Thread Count: " + ThreadCount);

                Console.SetCursorPosition(0, CursorPos + 2);
                Console.WriteLine("Iteration: " + CurrentIteration);
           
                Console.SetCursorPosition(0, CursorPos + 3);
                Console.WriteLine("Susceptible People Count: " + SusceptibleCount + "/ " + PeopleCount + "  ( " + ((double)SusceptibleCount / PeopleCount) * 100 + " % )           ");

                Console.SetCursorPosition(0, CursorPos + 4);
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Infected People Count: " + InfectedCount + "/ " + PeopleCount + "  ( " + ((double)InfectedCount / PeopleCount) * 100 + " % )           ");
                Console.ForegroundColor = ConsoleColor.Blue;

                Console.SetCursorPosition(0, CursorPos + 5);
                Console.WriteLine("Recovered People Count: " + RecoveredCount + "/ " + PeopleCount + "  ( " + ((double)RecoveredCount / PeopleCount) * 100 + " % )           ");

                Console.ResetColor();
            }

            

            IsPrinterDone = true;
            EndCheck();
            
        }

        public abstract void CalculateInfection(ConcurrentDictionary<int, Person> personDictionary, Person person);


        public virtual void ThreadMethod(ConcurrentDictionary<int, Person> personDictionary)
        {
            int CursorPos = isMapVisible ? MapYMax : 0;

            ThreadLocal<int> index = new ThreadLocal<int>();
            index.Value = Threads.FindIndex(x => x.Id == Task.CurrentId);

            while (!ThreadMethodExitCondition(personDictionary))
            {

                lock (MyLocks.lockIsDoneMovementObject)
                {
                    IsDoneWithMovement[index.Value] = false;
                }
                PeopleStatusUpdate(personDictionary);

                MovePeople(personDictionary);


                lock (MyLocks.lockIsDoneMovementObject)
                {
                    IsDoneWithMovement[index.Value] = true;

                }

                while (IsDoneWithMovement[index.Value] && !IsPrinterDone)
                {


                }
                
            }
        }

        public abstract void EndCheck();

        public void PeopleStatusUpdate(ConcurrentDictionary<int, Person> personDictionary)
        {
            foreach (var item in personDictionary.Values)
            {
                item.StatusUpdate();         
            }
        }

        public abstract bool ThreadMethodExitCondition(ConcurrentDictionary<int, Person> personDictionary);
    }
}
