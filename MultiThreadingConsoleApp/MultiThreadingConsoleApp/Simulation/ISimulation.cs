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
        void EndCheck();
        bool CheckIsDone();
        void StartSimulation();
        void StartTimer();
        void StartPrinter();
        void Synchronize(bool isMapVisible);
        List<Person> InfectPerson(Point pos);
        void CalculateInfection(ConcurrentDictionary<int, Person> personDictionary, Person person);
        void ThreadMethod(ConcurrentDictionary<int, Person> personDictionary);
        bool ThreadMethodExitCondition(ConcurrentDictionary<int, Person> personDictionary);
        int getInfectedPeopleCount(ConcurrentDictionary<int, Person> personDictionary);
        int getSusceptiblePeopleCount(ConcurrentDictionary<int, Person> personDictionary);
        int getRecoveredPeopleCount(ConcurrentDictionary<int, Person> personDictionary);
        void MovePeople(ConcurrentDictionary<int, Person> personDictionary);
        ConcurrentDictionary<int, Person> InitPersonDictionary(List<Person> personList);
        void PeopleStatusUpdate(ConcurrentDictionary<int, Person> personDictionary);


    }

}
