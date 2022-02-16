using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MultiThreadingConsoleApp
{
   public interface ISimulation
    {

        bool IsDone();

        void StartSimulation();

        void StartTimer();

        void StartPrinter();

        void PrintMap(ConcurrentDictionary<int, Person> globalPersonDictionary, bool isMapVisible);
        List<Person> InfectPerson(ConcurrentDictionary<int, Person> globalPersonDictionary, Point pos);
       
        void Thinking();

        void ThreadMethod(ConcurrentDictionary<int, Person> personDictionary);


        int getInfectedPeopleCount(ConcurrentDictionary<int, Person> personDictionary);


        void MovePeople(ConcurrentDictionary<int, Person> personDictionary);

        ConcurrentDictionary<int, Person> InitPersonDictionary(List<Person> personList);

    }

}
