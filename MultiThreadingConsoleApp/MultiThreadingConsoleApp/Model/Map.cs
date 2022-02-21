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

        public void UpdateMap(ConcurrentDictionary<int, Person> personDictionary, List<int> counts)
        {
            foreach (var item in personDictionary)
            {
                Point temp = item.Value.Position;
                int count = counts[item.Key];
                points[temp.X, temp.Y] = count.ToString();
               
                lock (MyLocks.lockConsoleObject) {
                    ConsoleColor c = item.Value.Color;
                    Console.ForegroundColor = c;
                    Console.SetCursorPosition(item.Value.PreviousPosition.X, item.Value.PreviousPosition.Y);
                    Console.Write("    ");
                    Console.SetCursorPosition(temp.X, temp.Y);
                    Console.Write("[" + points[temp.X, temp.Y] + "]");
                    Console.ResetColor();
                }
            }
        }
    }
}
