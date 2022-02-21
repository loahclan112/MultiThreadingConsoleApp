using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace MultiThreadingConsoleApp
{
    public class Data
    {
        private int mapX;
        private int mapY;
        private List<Person> personList = new List<Person>();
        private double recoveryRate;
        private double infectionRate;

        public int MapX { get => mapX; set => mapX = value; }
        public int MapY { get => mapY; set => mapY = value; }
        public List<Person> PersonList { get => personList; set => personList = value; }
        public double RecoveryRate { get => recoveryRate; set => recoveryRate = value; }
        public double InfectionRate { get => infectionRate; set => infectionRate = value; }
        public List<int> SusceptibleCount { get => susceptibleCount; set => susceptibleCount = value; }
        public List<int> InfectedCount { get => infectedCount; set => infectedCount = value; }
        public List<int> RecoveredCount { get => recoveredCount; set => recoveredCount = value; }

        private List<int> susceptibleCount = new List<int>();
        private List<int> infectedCount = new List<int>();
        private List<int> recoveredCount = new List<int>();
        public Data()
        {

        }

        public Data(int mapX, int mapY, double infectionRate, double recoveryRate, List<Person> personList)
        {
            this.MapX = mapX;
            this.MapY = mapY;
            this.PersonList = personList;
            this.InfectionRate = infectionRate;
            this.RecoveryRate = recoveryRate;
        }

        public Data LoadData(List<string> lines) {
            if (lines == null || lines.Count <=0)
            {
                return new Data();
            }
            Data data = new Data(Convert.ToInt32(lines[0]), Convert.ToInt32(lines[1]), Convert.ToDouble(lines[2]), Convert.ToInt32(lines[3]), new List<Person>());

            List<Person> personlistTemp = new List<Person>();
            Person temp;
            StatusEnum status;
            List<Point> temppositions;
            for (int i = 4; i < lines.Count; i++)
            {
                temppositions = new List<Point>();
                status = (StatusEnum)Enum.Parse(typeof(StatusEnum),lines[i].Split('|')[0]);
                foreach (var item in lines[i].Split('|')[1].Split(' '))
                {
                    temppositions.Add(new Point(item));
                }

                temp = new Person(status,temppositions);
                personlistTemp.Add(temp);
            }

            data.PersonList = personlistTemp;
            return data;
        }

        public List<string> SaveContent()
        {
            List<string> lines = new List<string>();
            lines.Add(MapX.ToString());
            lines.Add(MapY.ToString());
            lines.Add(InfectionRate.ToString());
            lines.Add(RecoveryRate.ToString());

            string personData = String.Empty;
            foreach (var item in this.PersonList)
            {
                personData += item.Status + "|";
                personData += String.Join(" ", item.DonePositions.Select(z => z.ToString()));
                lines.Add(personData);
                personData = String.Empty;
            }

            return lines;
        }

        public List<string> SaveCountContent()
        {
            List<string> lines = new List<string>();
            
            for (int i = 0; i < infectedCount.Count; i++)
            {
                lines.Add(susceptibleCount[i]+" "+infectedCount[i]+" "+recoveredCount[i]);
            }

            return lines;
        }
    }
}
