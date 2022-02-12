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

    public static class MyLocks {

        public static object lockConsoleObject = new object();
        public static object lockInfectedCountObject = new object();
        public static object lockUpdateMapObject = new object();


    }

    public readonly struct MyInt
    {

        public MyInt(int i)
        {
            this.I = i;
        }

        public int I { get; }
    }



    class Program
    {
        public static Random random = new Random();

        public const int mapXMax = 100;
        public const int mapYMax = 50;

        public const int peopleCount = 10000;
        public static Stopwatch sw = new Stopwatch();

        public static List<Thread> threads = new List<Thread>();

        public static ConcurrentDictionary<int, Person> personDictionary;

        public static int infectedCount = 0;

        public static Map map;

        public static Thread timerThread;

        public static void StartTimer() {

          
            sw.Start();
            while (true) {
                Thread.Sleep(1000);
                lock (MyLocks.lockConsoleObject)
                {
                   
                    Console.SetCursorPosition(0, mapYMax + 2);
                    Console.WriteLine("Elapsed Seconds: " + (double)sw.ElapsedMilliseconds / 1000 +" s");
                }
            }

            
        }

        static void Main(string[] args)
        {
            Console.CursorVisible = false;
            timerThread = new Thread(StartTimer);

            int threadCount = 2;
          
            map = new Map(mapXMax, mapYMax);

            personDictionary = InitPersonDictionary(peopleCount);

            InfectRandomPeople(personDictionary, 50);
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

            int k = 0;

            //TODO: START THREAD WITHOUT INCREASEING THE VALUE!!!!!!!!!!!!!!!!!
            foreach (var item in listDictionary)
            {
               
                threads.Add(new Thread(() => MainMethod(item, personDictionary,  new MyInt(Convert.ToInt32(k.ToString())))));
                threads[k].Start();
                k+=1;
            }

            timerThread.Start();

            Console.ReadLine();
        }

        public static void MainMethod(ConcurrentDictionary<int, Person> personDictionary, ConcurrentDictionary<int, Person> globalPersonDictionary,  MyInt threadId) {
            int infectedcount = 0;
            while (infectedcount != peopleCount)
            {
                //Thread.Sleep(300);

                MovePeople(personDictionary);
                 int temp = getInfectedPeopleCount(globalPersonDictionary);
                lock (MyLocks.lockInfectedCountObject) {
                    infectedcount = temp;
                }

                    map.UpdateMap(globalPersonDictionary);

               
                lock (MyLocks.lockConsoleObject)
                {
                    Console.SetCursorPosition(0, mapYMax + 1);
                    Console.WriteLine("Infected People Count: " + infectedcount + "/ " + peopleCount + "  ( " + ((double)infectedcount / peopleCount) * 100 + " % )");
                }

                // map.PrintMap();

            }

           

            timerThread.Abort();

            //threads[threadId.I - 1].Join();
        }


        public static int getInfectedPeopleCount(ConcurrentDictionary<int, Person> personDictionary)
        {

            return personDictionary.Where(x => x.Value.IsInfected).ToList().Count;
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

        public static void InfectOnePerson(ConcurrentDictionary<int, Person> personDictionary, int id)
        {
            personDictionary[id].IsInfected = true;
        }

        public static void MovePeople(ConcurrentDictionary<int, Person> personDictionary)
        {

            foreach (var item in personDictionary)
            {
                RandomMove(item.Value, mapXMax, mapYMax);
            }

        }

        public static ConcurrentDictionary<int, Person> InitPersonDictionary(int peopleCount)
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

    }


}
