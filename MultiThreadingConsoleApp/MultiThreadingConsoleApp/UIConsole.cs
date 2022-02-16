using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiThreadingConsoleApp
{
    class UIConsole
    {
        public ISimluation SimulationSelector() {



            return new SimulationRandom();

            return new Simulation();
        }

    }
}
