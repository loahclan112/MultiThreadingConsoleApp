using System;

namespace MultiThreadingConsoleApp
{
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
