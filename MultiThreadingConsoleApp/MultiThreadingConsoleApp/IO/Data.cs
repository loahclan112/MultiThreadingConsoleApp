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

        public int MapX { get => mapX; set => mapX = value; }
        public int MapY { get => mapY; set => mapY = value; }
        public List<Person> PersonList { get => personList; set => personList = value; }

        public Data()
        {

        }

        public Data(int mapX, int mapY, List<Person> personList)
        {
            this.MapX = mapX;
            this.MapY = mapY;
            this.PersonList = personList;
        }

        public Data LoadData(List<string> lines) {
            Data data = new Data(Convert.ToInt32(lines[0]), Convert.ToInt32(lines[1]), new List<Person>());

            List<Person> personlistTemp = new List<Person>();
            Person temp;
            StatusEnum status;
            List<Point> temppositions;
            for (int i = 2; i < lines.Count; i++)
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
    }
}
