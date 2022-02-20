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
                simulation.StartSimulation();

                while (!simulation.CheckIsDone())
                {

                }

            } while (uIConsole.StartAgain()) ;

        }
    }
}
