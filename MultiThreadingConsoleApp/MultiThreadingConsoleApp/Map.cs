using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace MultiThreadingConsoleApp
{
    public class Map
    {

        string[,] points;

        public Map(string[,] points)
        {
            this.points = points;
        }

        public Map(int x, int y)
        {

            this.points = InitMap(x, y);
        }

        private string[,] InitMap(int x, int y)
        {
            string[,] map = new string[x, y];
            for (int i = 0; i < x; i++)
            {
                for (int j = 0; j < y; j++)
                {
                    map[i, j] = " ";
                }
            }

            return map;
        }

        public void UpdateMap(ConcurrentDictionary<int, Person> personDictionary)
        {
            CleanUpMap2(personDictionary);
            //ResetMap();
            foreach (var item in personDictionary)
            {
                Point temp = item.Value.Position;

                int count = InfectPerson(personDictionary, temp).Count;
                lock (MyLocks.lockUpdateMapObject) {
                    points[temp.X, temp.Y] = count.ToString();
                }

                lock (MyLocks.lockConsoleObject) {
                    ConsoleColor c = item.Value.Color;
                    Console.ForegroundColor = c;
                    Console.SetCursorPosition(temp.X, temp.Y);
                    Console.Write("[" + points[temp.X, temp.Y] + "]");
                    Console.ResetColor();
                }
            }

        }

        public void UpdateMapParallel(ConcurrentDictionary<int, Person> personDictionary)
        {
            CleanUpMap2(personDictionary);
            //ResetMap();
            foreach (var item in personDictionary)
            {
                Point temp = item.Value.Position;

                int count = InfectPerson(personDictionary, temp).Count;
                lock (MyLocks.lockUpdateMapObject)
                {
                    points[temp.X, temp.Y] = count.ToString();
                }

                lock (MyLocks.lockConsoleObject)
                {
                    ConsoleColor c = item.Value.Color;
                    Console.ForegroundColor = c;
                    Console.SetCursorPosition(temp.X, temp.Y);
                    Console.Write("[" + points[temp.X, temp.Y] + "]");
                    Console.ResetColor();
                }
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

        private void ResetMap()
        {
            int xMax = points.GetLength(0);
            int yMax = points.GetLength(1);
            for (int i = 0; i < xMax+3; i++)
            {
                for (int j = 0; j < yMax; j++)
                {
                    lock (MyLocks.lockConsoleObject)
                    {
                        Console.SetCursorPosition(i, j);
                        Console.Write(" ");
                    }

                }
            }

        }

        private void CleanUpMapWithEmpty(ConcurrentDictionary<int, Person> personDictionary)
        {
            List<Point> personPoints = personDictionary.Values.Select(x => x.Position).ToList();

            List<Point> personPreviousPoints = personDictionary.Values.Select(x => x.PreviousPosition).ToList();

            foreach (Point item in personPreviousPoints)
            {
                lock (MyLocks.lockConsoleObject)
                {
                    Console.SetCursorPosition(item.X, item.Y);
                    Console.Write(" ");
                    Console.SetCursorPosition(item.X + 1, item.Y);
                    Console.Write(" ");
                    Console.SetCursorPosition(item.X + 2, item.Y);
                    Console.Write(" ");
                }
            }

            for (int i = 0; i < points.GetLength(1); i++)
            {
                lock (MyLocks.lockConsoleObject)
                {
                    Console.SetCursorPosition(points.GetLength(0), i);
                    Console.Write(" ");
                    Console.SetCursorPosition(points.GetLength(0) + 1, i);
                    Console.Write(" ");
                    Console.SetCursorPosition(points.GetLength(0) + 2, i);
                    Console.Write(" ");
                }

            }
        }

        private void CleanUpMap2(ConcurrentDictionary<int, Person> personDictionary)
        {
            List<Point> personPoints = personDictionary.Values.Select(x => x.Position).ToList();

            List<Point> personPreviousPoints = personDictionary.Values.Select(x => x.PreviousPosition).ToList();

            if (personPreviousPoints[0] == null)
            {
                return;
            }

            foreach (Point item in personPreviousPoints)
            {
                    lock (MyLocks.lockConsoleObject)
                    {
                        Console.SetCursorPosition(item.X, item.Y);
                        Console.Write(" ");
                        Console.SetCursorPosition(item.X + 1, item.Y);
                        Console.Write(" ");
                        Console.SetCursorPosition(item.X + 2, item.Y);
                        Console.Write(" ");
                        Console.SetCursorPosition(item.X + 3, item.Y);
                        Console.Write(" ");
                    }


            }

            for (int i = 0; i < points.GetLength(1); i++)
            {
                lock (MyLocks.lockConsoleObject)
                {
                    Console.SetCursorPosition(points.GetLength(0), i);
                    Console.Write(" ");
                    Console.SetCursorPosition(points.GetLength(0) + 1, i);
                    Console.Write(" ");
                    Console.SetCursorPosition(points.GetLength(0) + 2, i);
                    Console.Write(" ");
                }

            }
        }

        //REFACTOR THIS
        private void CleanUpMap(ConcurrentDictionary<int, Person> personDictionary)
        {

            List<Point> personPoints = personDictionary.Values.Select(x => x.Position).ToList();


            for (int i = 0; i < points.GetLength(0); i++)
            {
                for (int j = 0; j < points.GetLength(1); j++)
                {
                    if (!(personPoints.Contains(new Point(i, j)) || personPoints.Contains(new Point(i - 1, j)) || personPoints.Contains(new Point(i - 2, j))))
                    {
                        lock (MyLocks.lockConsoleObject)
                        {
                            points[i, j] = " ";
                            Console.SetCursorPosition(i, j);
                            Console.Write(points[i, j]);
                            if (i - 1 >= 0)
                            {
                                Console.SetCursorPosition(i - 1, j);
                                points[i - 1, j] = " ";
                                Console.Write(points[i - 1, j]);
                            }
                            if (i - 2 >= 0)
                            {
                                Console.SetCursorPosition(i - 2, j);
                                points[i - 2, j] = " ";
                                Console.Write(points[i - 2, j]);
                            }
                        }
                     
                    }
                }
            }

            for (int i = 0; i < points.GetLength(1); i++)
            {
                lock (MyLocks.lockConsoleObject)
                {
                    Console.SetCursorPosition(points.GetLength(0), i);
                    Console.Write(" ");
                    Console.SetCursorPosition(points.GetLength(0) + 1, i);
                    Console.Write(" ");
                }

            }
        }

        public void PrintMap()
        {
            if (this.points == null || this.points.Length == 0)
            {
                return;
            }

            for (int i = 0; i < points.GetLength(0); i++)
            {
                Console.WriteLine();
                for (int j = 0; j < points.GetLength(1); j++)
                {
                    Console.Write(points[i, j]);
                }
            }

        }

    }


}
