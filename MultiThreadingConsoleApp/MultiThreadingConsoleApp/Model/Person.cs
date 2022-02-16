using System;
using System.Collections.Generic;

namespace MultiThreadingConsoleApp
{
    public class Person
    {
        public static int commonId = 0;
        int id;
        public List<Point> remainingPositions = new List<Point>();
        private List<Point> donePositions = new List<Point>();

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
        public List<Point> DonePositions { get => donePositions; set => donePositions = value; }

        public Person(int x, int y)
        {
            this.Id = commonId;
            this.Position = new Point(x, y);
            this.previousPosition = this.position;
            DonePositions.Add(this.Position);
            commonId++;
        }

        public Person(Point point)
        {
            this.Id = commonId;
            this.Position = point;
            this.previousPosition = this.position;
            DonePositions.Add(this.Position);
            commonId++;
        }

        public Person(bool isInfected, List<Point> points) 
        {
          
            this.Id = commonId;
            this.IsInfected = isInfected;
            this.remainingPositions = points;
            this.Position = points[0];
            this.previousPosition = this.Position;
            this.remainingPositions.RemoveAt(0);
            DonePositions.Add(this.Position);
            commonId++;
        }

        public Point Move(int dx, int dy, int boundaryX, int boundaryY)
        {
            this.previousPosition = this.position;
            if (!MoveValidation(this.position.X, this.position.Y, dx, dy, boundaryX, boundaryY)) { return null; }

            this.position = new Point(this.position.X + dx, this.position.Y + dy);

            return this.position;
        }

        public Point Move()
        {
            this.previousPosition = this.position;
           

            this.position = this.remainingPositions[0];
            this.remainingPositions.RemoveAt(0);
            this.DonePositions.Add(this.position);
            return this.position;
        }

        private bool MoveValidation(int posX, int posY, int dx, int dy, int boundaryX, int boundaryY)
        {
            return 0 <= posX + dx && posX + dx < boundaryX &&
                   0 <= posY + dy && posY + dy < boundaryY;

        }

    }

}
