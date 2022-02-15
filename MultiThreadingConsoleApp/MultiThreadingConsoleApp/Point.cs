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

        public double DistanceFromOtherPoint(Point point) 
        {
            double distance = Math.Sqrt(Math.Pow(this.x - point.X, 2) + Math.Pow(this.Y - point.Y, 2));

            return distance;
        }

        public double AngleBetweenOtherPoint(Point point) 
        {
            var radian = Math.Atan2((point.Y - this.Y), (point.X - this.X));
            var angle = (radian * (180 / Math.PI) + 360) % 360;

            return angle;
        }

        //get distance as well
        public Point GetMovementDirectionToOtherPoint(Point point) {

            int x = 0;
            int y = 0;

            /*-1,-1
             * -1,0
             * -1,1
             * 0,-1
             * 0,0
             * 0,1
             * 1,-1
             * 1,0
             * 1,-1
             * */

            if (point.X < this.X && point.Y < this.Y)
            {
                x = -1;
                y = -1;
            }
            else if (point.X < this.X && point.Y == this.Y) 
            {
                x = -1;
                y = 0;
            }
            else if (point.X < this.X && point.Y > this.Y)
            {
                x = -1;
                y = 1;
            }

            else if (point.X == this.X && point.Y < this.Y)
            {
                x = 0;
                y = -1;
            }
            else if (point.X == this.X && point.Y == this.Y)
            {
                x = 0;
                y = 0;
            }
            else if (point.X == this.X && point.Y > this.Y)
            {
                x = 0;
                y = 1;
            }

            else if (point.X > this.X && point.Y < this.Y)
            {
                x = 1;
                y = -1;
            }

            else if (point.X > this.X && point.Y == this.Y)
            {
                x = 1;
                y = 0;
            }
            else if (point.X > this.X && point.Y > this.Y)
            {
                x = 1;
                y = 1;
            }

            return new Point(x,y);
        
        }
    }
}
