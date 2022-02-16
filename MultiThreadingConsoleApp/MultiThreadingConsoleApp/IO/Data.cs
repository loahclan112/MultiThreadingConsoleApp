using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace MultiThreadingConsoleApp
{
    public class Data
    {

        public  int mapX;
        public  int mapY;

        public  List<Person> personList = new List<Person>();
        public Data()
        {

        }

        public Data(int mapX, int mapY, List<Person> personList)
        {
            this.mapX = mapX;
            this.mapY = mapY;
            this.personList = personList;
        }

        public Data LoadData(List<string> lines) {
            Data data = new Data(Convert.ToInt32(lines[0]), Convert.ToInt32(lines[1]), new List<Person>());

            List<Person> personlistTemp = new List<Person>();
            Person temp;
            bool isInfectedtemp;
            List<Point> temppositions;
            for (int i = 2; i < lines.Count; i++)
            {
                temppositions = new List<Point>();
                isInfectedtemp = lines[i].Split('|')[0] == "True" ? true : false;
                foreach (var item in lines[i].Split('|')[1].Split(' '))
                {
                    temppositions.Add(new Point(item));
                }

                temp = new Person(isInfectedtemp,temppositions);
                personlistTemp.Add(temp);
            }

            data.personList = personlistTemp;
            return data;
        }

        public List<string> SaveContent()
        {
            List<string> lines = new List<string>();
            lines.Add(mapX.ToString());
            lines.Add(mapY.ToString());

            string personData = String.Empty;
            foreach (var item in this.personList)
            {
                personData += item.IsInfected + "|";
                personData += String.Join(" ", item.DonePositions.Select(z => z.ToString()));
                lines.Add(personData);
                personData = String.Empty;
            }

            return lines;
        }
    }
}
