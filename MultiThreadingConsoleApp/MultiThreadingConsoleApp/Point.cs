using System;

namespace MultiThreadingConsoleApp
{
    public class Point
    {
        int x;
        int y;



        public Point(string pointString) 
        {
            string[] splitstring = pointString.Split(',');

            this.x =  Convert.ToInt32(splitstring[0].Split('(')[1]);
            this.y = Convert.ToInt32(splitstring[1].Split(')')[0]);
        }

        public Point(int x, int y)
        {
            this.X = x;
            this.Y = y;
        }

        public int X { get => x; set => x = value; }
        public int Y { get => y; set => y = value; }

        public override string ToString()
        {
            return $"({this.X},{this.Y})";
        }
    }
}
