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

            do
            {
                Console.Clear();
                ISimulation simulation = uIConsole.SimulationSelector();

                Console.Clear();

                /*
               ISimluation sim = new Simulation(6,true);
               ISimluation simrand = new SimulationRandom(2,20,10,20,true,false);
                */
                simulation.StartSimulation();

                while (!simulation.IsDone())
                {

                }

            } while (uIConsole.StartAgain()) ;

        }
    }
}
