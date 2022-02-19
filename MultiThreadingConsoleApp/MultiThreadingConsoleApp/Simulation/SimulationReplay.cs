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
        public  int MapXMax = 0;
        public Data data;
        public List<StatusEnum> infectedPeopleStart;
        public Task timerThread;
        public Task printerThread;


        public SimulationReplay(int threadCount = 1, bool isMapVisible=true) 
        {
            ThreadCount = ThreadCount;
            IsMapVisible = isMapVisible; 
        }

        public override void StartSimulation() {
            Console.CursorVisible = false;
            timerThread = new Task(StartTimer, TaskCreationOptions.LongRunning);
            printerThread = new Task(StartPrinter, TaskCreationOptions.LongRunning);
            IsDoneWithMovement = new List<bool>();
            data = new Data();

            data = data.LoadData(FileHandler.ReadFromFile());

            MapXMax = data.MapX;
            MapYMax = data.MapY;

            PeopleCount = data.PersonList.Count;
            InfectedCount = data.PersonList.Where(x => x.Status == StatusEnum.Infected).ToList().Count;

            Map = new Map(MapXMax, MapYMax);

            GlobalPersonDictionary = InitPersonDictionary(data.PersonList);


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

            foreach (var item in listDictionary)
            {
                IsDoneWithMovement.Add(false);
               Threads.Add(new Task(() => ThreadMethod(item), TaskCreationOptions.LongRunning));
            }

            foreach (var item in Threads)
            {
                item.Start();
            }

            timerThread.Start();
            printerThread.Start();        
        }


        public override void EndCheck() {

            if (Source.IsCancellationRequested)
            {
                data.PersonList = GlobalPersonDictionary.Values.ToList();
                for (int i = 0; i < infectedPeopleStart.Count; i++)
                {
                    data.PersonList[i].Status = infectedPeopleStart[i];
                }
                FileHandler.WriteToFile(data.SaveContent(), false);
                Person.commonId = 0;
                IsDone = true;
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
                if (item.Value.remainingPositions.Count>0)
                {
                    item.Value.Move();
                }
            }
        }
    }


}
