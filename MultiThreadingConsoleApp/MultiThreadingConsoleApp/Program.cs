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
    class Program
    {
        static void Main(string[] args)
        {

            UIConsole uIConsole = new UIConsole();

           ISimluation sim = new Simulation(6,true);
           ISimluation simrand = new SimulationRandom(2,20,10,20,true,false);

            sim.StartSimulation();

            Console.ReadLine();
        }
    }
}
