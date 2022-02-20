using System;
using System.Collections.Generic;

namespace MultiThreadingConsoleApp
{
    public class Person
    {
        public static int commonId = 0;
        int id;
        private List<Point> remainingPositions = new List<Point>();
        private List<Point> donePositions = new List<Point>();

        Point previousPosition;
        Point position;
        int texture = 1;
        ConsoleColor color = ConsoleColor.White;
        StatusEnum status = StatusEnum.Susceptible;
        int speed = 1;
        double recoveryRate = 0;
        double infectionRate = 1;


        public int Id { get => id; set => id = value; }
        public Point Position { get => position; set => position = value; }
        public int Texture { get => texture; set => texture = value; }
        public int Speed { get => speed; set => speed = value; }
        public ConsoleColor Color { get => color; set => color = value; }
        public StatusEnum Status { get => status;
            set
            {
                status = value;

                switch (status)
                {
                    case StatusEnum.Susceptible:
                        Color = ConsoleColor.White;
                        break;
                    case StatusEnum.Infected:
                        Color = ConsoleColor.Red;
                        break;
                    case StatusEnum.Recovered:
                        Color = ConsoleColor.Blue;
                        break;
                    default:
                        break;
                }
            }

        }
        public Point PreviousPosition { get => previousPosition; set => previousPosition = value; }
        public List<Point> DonePositions { get => donePositions; set => donePositions = value; }
        public double RecoveryRate { get => recoveryRate; set => recoveryRate = value; }
        public double InfectionRate { get => infectionRate; set => infectionRate = value; }
        public List<Point> RemainingPositions { get => remainingPositions; set => remainingPositions = value; }

        public Person(Point point)
        {
            this.Id = commonId;
            this.Position = point;
            this.previousPosition = this.position;
            DonePositions.Add(this.Position);
            commonId++;
        }

        public Person(StatusEnum status, List<Point> points) 
        {
          
            this.Id = commonId;
            this.Status = status;
            this.RemainingPositions = points;
            this.Position = points[0];
            this.previousPosition = this.Position;
            this.RemainingPositions.RemoveAt(0);
            DonePositions.Add(this.Position);
            commonId++;
        }

        public void StatusUpdate() {
            if (this.Status == StatusEnum.Infected)
            {
                if (RecoveryRate <= 0)
                {
                    this.Status = StatusEnum.Recovered;
                    return;
                }
                    RecoveryRate--;
            }
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
           

            this.position = this.RemainingPositions[0];
            this.RemainingPositions.RemoveAt(0);
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
