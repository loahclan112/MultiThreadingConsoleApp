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
     public class SimulationReplay : SimulationBase
     {
        public SimulationReplay(int threadCount = 1, bool isMapVisible=true) 
        {
            ThreadCount = threadCount;
            IsMapVisible = isMapVisible; 
        }

        public override void StartSimulation() {
            Console.CursorVisible = false;
            IsDoneWithMovement = new List<bool>();

            TimerThread = new Task(StartTimer, Source.Token, TaskCreationOptions.LongRunning);
            PrinterThread = new Task(StartPrinter, Source.Token, TaskCreationOptions.LongRunning);

            Data = new Data();
            Data = Data.LoadData(FileHandler.ReadFromFile());
            MapXMax = Data.MapX;
            MapYMax = Data.MapY;
            RecoveryRate = Data.RecoveryRate;
            InfectionRate = Data.InfectionRate;
            PeopleCount = Data.PersonList.Count;
            InfectedCount = Data.PersonList.Where(x => x.Status == StatusEnum.Infected).ToList().Count;

            Map = new Map(MapXMax, MapYMax);

            GlobalPersonDictionary = InitPersonDictionary(Data.PersonList);
            InfectedPeopleStart = new List<StatusEnum>();

            foreach (var item in GlobalPersonDictionary.Values)
            {
                InfectedPeopleStart.Add(item.Status);
            }

            ListDictionary = new List<Dictionary<int, Person>>();
            Dictionary<int, Person> personDictionarytemp = new Dictionary<int, Person>();

            int personDistributedCount = 0;

            for (int i = 0; i < ThreadCount; i++)
            {
                for (int j = 0; j < PeopleCount / ThreadCount; j++)
                {
                    personDictionarytemp.Add(i * (PeopleCount / ThreadCount) + j, GlobalPersonDictionary[i * (PeopleCount / ThreadCount) + j]);
                    personDistributedCount++;

                }
                ListDictionary.Add(new Dictionary<int, Person>(personDictionarytemp));
                personDictionarytemp = new Dictionary<int, Person>();
            }

            for (int k = 1; k <= PeopleCount - personDistributedCount; k++)
            {

                ListDictionary[ThreadCount - 1].Add(PeopleCount - k, GlobalPersonDictionary[PeopleCount - k]);
            }

            foreach (var item in ListDictionary)
            {
               IsDoneWithMovement.Add(false);
               Threads.Add(new Task(() => ThreadMethod(new ConcurrentDictionary<int,Person>(item)), TaskCreationOptions.LongRunning));
            }

            foreach (var item in Threads)
            {
                item.Start();
            }

            TimerThread.Start();
            PrinterThread.Start();        
        }


        public override void EndCheck() {
            if (InfectedCount >= PeopleCount || InfectedCount <= 0)
            {
                Source.Cancel();

            }
            if (Source.IsCancellationRequested)
            {
                Task.WaitAll(Threads.ToArray());
                for (int i = 0; i < Threads.Count; i++)
                {
                    Threads[i].Dispose();
                }
                Data.PersonList = GlobalPersonDictionary.Values.ToList();
                for (int i = 0; i < InfectedPeopleStart.Count; i++)
                {
                    Data.PersonList[i].Status = InfectedPeopleStart[i];
                }
                FileHandler.WriteToFile(Data.SaveContent(), false);
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

        public override void CalculateInfection(ConcurrentDictionary<int, Person> personDictionary, Person person)
        {
            if (IsMapVisible)
            {
                for (int i = 0; i < 100; i++)
                {
                    person.InfectionRate = (double)StaticRandom.Rand(5, 11) / 10;
                }
            }
            else {
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
                CalculateInfection(personDictionary, item);
                if (item.RemainingPositions.Count>0)
                {
                   item.Move();
                }
            }
        }

        public bool ThreadMethodExitCondition2(ConcurrentDictionary<int, Person> personDictionary) 
        {
            return personDictionary.Values.Where(x => x.RemainingPositions.Count > 0).Count() <= 0;
        }

        public override bool ThreadMethodExitCondition(ConcurrentDictionary<int, Person> personDictionary)
        {
            return Source.IsCancellationRequested;
        }
    }
}
