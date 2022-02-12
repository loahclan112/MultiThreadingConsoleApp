@@ -0,0 + 1,360 @@
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MultiThreadingConsoleApp
{
    class Program
    {
        public static Random random = new Random();

        public const int mapXMax = 200;
        public const int mapYMax = 100;

        public const int peopleCount = 10000;


        static void Main(string[] args)
        {

            Console.CursorVisible = false;


            Map map = new Map(mapXMax, mapYMax);

            Dictionary<int, Person> personDictionary = InitPersonDictionary(peopleCount);

            InfectRandomPeople(personDictionary, 50);

            int infectedcount = 0;

            while (((double)infectedcount / peopleCount) < 1)
            {

                //Thread.Sleep(100);
                MovePeople(personDictionary);
                map.UpdateMap(personDictionary);

                Console.SetCursorPosition(0, mapYMax + 1);
                infectedcount = getInfectedPeopleCount(personDictionary);
                Console.WriteLine("Infected People Count: " + infectedcount + "/ " + peopleCount + "  ( " + ((double)infectedcount / peopleCount) * 100 + " % )");
                // map.PrintMap();
            }



            Console.ReadLine();
        }


        public static int getInfectedPeopleCount(Dictionary<int, Person> personDictionary)
        {

            return personDictionary.Where(x => x.Value.IsInfected).ToList().Count;
        }

        public static void InfectRandomPeople(Dictionary<int, Person> personDictionary, int count)
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

        public static void InfectOnePerson(Dictionary<int, Person> personDictionary, int id)
        {
            personDictionary[id].IsInfected = true;
        }

        public static void MovePeople(Dictionary<int, Person> personDictionary)
        {

            foreach (var item in personDictionary)
            {
                RandomMove(item.Value, mapXMax, mapYMax);
            }

        }

        public static Dictionary<int, Person> InitPersonDictionary(int peopleCount)
        {
            Dictionary<int, Person> personDictionary = new Dictionary<int, Person>();

            for (int i = 0; i < peopleCount; i++)
            {
                Person temp = GeneratePersonInsideBoundaries(0, mapXMax, 0, mapYMax);
                personDictionary.Add(temp.Id, temp);
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
    public class Point
    {
        int x;
        int y;

        public Point(int x, int y)
        {
            this.X = x;
            this.Y = y;
        }

        public int X { get => x; set => x = value; }
        public int Y { get => y; set => y = value; }
    }

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

        public void UpdateMap(Dictionary<int, Person> personDictionary)
        {

            CleanUpMap2(personDictionary);
            foreach (var item in personDictionary)
            {
                Point temp = item.Value.Position;
                Console.SetCursorPosition(temp.X, temp.Y);

                int count = InfectPerson(personDictionary, temp).Count;

                if (count > 2)
                {

                    bool a = false;
                }

                ConsoleColor c = item.Value.Color;
                Console.ForegroundColor = c;

                points[temp.X, temp.Y] = count.ToString();
                Console.Write("[" + points[temp.X, temp.Y] + "]");

                Console.ResetColor();

            }
        }

        public List<Person> InfectPerson(Dictionary<int, Person> personDictionary, Point pos)
        {
            List<Person> infectedPersonList = personDictionary.Values.Where(x => Math.Abs(x.Position.X - pos.X) < 2 && Math.Abs(x.Position.Y - pos.Y) < 1).ToList();
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
            Console.Clear();
        }


        private void CleanUpMap2(Dictionary<int, Person> personDictionary)
        {
            List<Point> personPoints = personDictionary.Values.Select(x => x.Position).ToList();

            List<Point> personPreviousPoints = personDictionary.Values.Select(x => x.PreviousPosition).ToList();

            foreach (Point item in personPreviousPoints)
            {
                Console.SetCursorPosition(item.X, item.Y);
                Console.Write(" ");
                Console.SetCursorPosition(item.X + 1, item.Y);
                Console.Write(" ");
                Console.SetCursorPosition(item.X + 2, item.Y);
                Console.Write(" ");
            }

            for (int i = 0; i < points.GetLength(1); i++)
            {
                Console.SetCursorPosition(points.GetLength(0), i);
                Console.Write(" ");
                Console.SetCursorPosition(points.GetLength(0) + 1, i);
                Console.Write(" ");
                Console.SetCursorPosition(points.GetLength(0) + 2, i);
                Console.Write(" ");
            }
        }

        //REFACTOR THIS
        private void CleanUpMap(Dictionary<int, Person> personDictionary)
        {

            List<Point> personPoints = personDictionary.Values.Select(x => x.Position).ToList();


            for (int i = 0; i < points.GetLength(0); i++)
            {
                for (int j = 0; j < points.GetLength(1); j++)
                {
                    if (!(personPoints.Contains(new Point(i, j)) || personPoints.Contains(new Point(i - 1, j)) || personPoints.Contains(new Point(i - 2, j))))
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

            for (int i = 0; i < points.GetLength(1); i++)
            {
                Console.SetCursorPosition(points.GetLength(0), i);
                Console.Write(" ");
                Console.SetCursorPosition(points.GetLength(0) + 1, i);
                Console.Write(" ");
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

    public class Person
    {
        public static int commonId = 0;
        int id;

        Point previousPosition;
        Point position;
        int texture = 1;
        ConsoleColor color = ConsoleColor.White;
        int speed = 1;
        bool isInfected = false;

        public int Id { get => id; set => id = value; }
        public Point Position { get => position; set => position = value; }
        public int Texture { get => texture; set => texture = value; }
        public int Speed { get => speed; set => speed = value; }
        public ConsoleColor Color { get => color; set => color = value; }
        public bool IsInfected { get => isInfected; set { isInfected = value; if (isInfected) Color = ConsoleColor.Red; } }

        public Point PreviousPosition { get => previousPosition; set => previousPosition = value; }

        public Person(int x, int y)
        {
            this.Id = commonId;
            this.Position = new Point(x, y);
            commonId++;
        }

        public Person(Point point)
        {
            this.Id = commonId;
            this.Position = point;
            commonId++;
        }



        public Point Move(int dx, int dy, int boundaryX, int boundaryY)
        {
            this.previousPosition = this.position;
            if (!MoveValidation(this.position.X, this.position.Y, dx, dy, boundaryX, boundaryY)) { return null; }

            this.position = new Point(this.position.X + dx, this.position.Y + dy);

            return this.position;
        }

        private bool MoveValidation(int posX, int posY, int dx, int dy, int boundaryX, int boundaryY)
        {
            return 0 <= posX + dx && posX + dx < boundaryX &&
                   0 <= posY + dy && posY + dy < boundaryY;

        }

    }


}