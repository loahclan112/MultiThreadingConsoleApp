using MultiThreadingConsoleApp.Simulation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiThreadingConsoleApp
{

    class UIConsole
    {
        public string GetUserInput() 
        {
            return Console.ReadLine();
        }
        private int InfectedCountSelect(int populationCount)
        {
            Console.Write("Select Infected Person Count (Suggested 1 - peopleCount):  ");
            string result = GetUserInput();

            int temp = -1;

            try
            {
                temp = ConvertToInt(result);
                if (temp < 1 || temp > populationCount)
                {
                    throw new InvalidInputException("Invalid Infected Person Count set to 1");
                }

            }
            catch (InvalidInputException e)
            {

                Console.WriteLine(e.msg);
                temp = 1;
            }

            return temp;
        }

        private int SimulationTypeSelect() 
        {
            Console.Write("Select Simulation type (0 - Replay, 1 - Generate):  ");
            string result = GetUserInput();

            int simulationType = -1;

            try
            {
                simulationType = ConvertToInt(result);
                if (simulationType < 0 || simulationType > 1)
                {
                    throw new InvalidInputException();
                }

            }
            catch (Exception e)
            {

                Console.WriteLine("Invalid input Simulation type set to normal");
                simulationType = 0;
            }

            return simulationType;
        }

        private int PeopleCountSelect()
        {
            Console.Write("Select Population Count (Suggested: 1 - 1000):  ");
            string result = GetUserInput();

            int temp = -1;

            try
            {
                temp = ConvertToInt(result);
                if (temp < 1 || temp > 100000)
                {
                    throw new InvalidInputException("Invalid input Population Count set to 20");
                }

            }
            catch (InvalidInputException e)
            {

                Console.WriteLine(e.msg);
                temp = 20;
            }

            return temp;
        }

        private double RecoveryRateSelect()
        {
            Console.Write("Select Recovery Rate (Suggested: 7 - 21):  ");
            string result = GetUserInput();

            double temp = -1;

            try
            {
                temp = ConvertToInt(result);
                if (temp < 1 || temp > 10000)
                {
                    throw new InvalidInputException("Invalid input Recovery rate set to 14");
                }

            }
            catch (InvalidInputException e)
            {

                Console.WriteLine(e.msg);
                temp = 14;
            }

            return temp;
        }

        private double InfectionRateSelect()
        {
            Console.Write("Select Infection Rate (Suggested: 0.0 - 1.0):  ");
            string result = GetUserInput();

            double temp = -1;

            try
            {
                temp = ConvertToDouble(result);
                if (temp < 0 || temp > 1)
                {
                    throw new InvalidInputException("Invalid input Infection rate set to 1");
                }

            }
            catch (InvalidInputException e)
            {

                Console.WriteLine(e.msg);
                temp = 1;
            }

            return temp;
        }

        private int ThreadCountSelect()
        {
            Console.Write("Select Thread Count (1 - 16):  ");
            string result = GetUserInput();

            int temp = -1;

            try
            {
                temp = ConvertToInt(result);
                if (temp < 1 || temp > 16)
                {
                    throw new InvalidInputException("Invalid input ThreadCount set to 1");
                }

            }
            catch (InvalidInputException e)
            {

                Console.WriteLine(e.msg);
                temp = 1;
            }

            return temp;
        }

        private int VisualizeFlagSelect() {

            Console.Write("Select Visualization (0 - off, 1 - on):  ");
            string result = GetUserInput();

            int temp = -1;

            try
            {
                temp = ConvertToInt(result);
                if (temp < 0 || temp > 1)
                {
                    throw new InvalidInputException("Invalid input Visualization turned ON");
                }

            }
            catch (InvalidInputException e)
            {

                Console.WriteLine(e.msg);
                temp = 1;
            }

            return temp;
        }
        private int MapXSelect()
        {

            Console.Write("Select Map X (Suggested: 10 - 200):  ");
            string result = GetUserInput();

            int temp = -1;

            try
            {
                temp = ConvertToInt(result);
                if (temp < 5 || temp > 20000)
                {
                    throw new InvalidInputException("Invalid Map X set to 150");
                }

            }
            catch (InvalidInputException e)
            {

                Console.WriteLine(e.msg);
                temp = 150;
            }

            return temp;
        }

        private int MapYSelect()
        {

            Console.Write("Select Map Y (Suggested: 10 - 100):  ");
            string result = GetUserInput();

            int temp = -1;

            try
            {
                temp = ConvertToInt(result);
                if (temp < 5 || temp > 10000)
                {
                    throw new InvalidInputException("Invalid Map Y set to 75");
                }

            }
            catch (InvalidInputException e)
            {

                Console.WriteLine(e.msg);
                temp = 75;
            }

            return temp;
        }
        private int ZombieModeSelect()
        {

            Console.Write("Select Infection Mode (0 - Random Mode, 1 - Zombie Mode):  ");
            string result = GetUserInput();

            int temp = -1;

            try
            {
                temp = ConvertToInt(result);
                if (temp < 0 || temp > 1)
                {
                    throw new InvalidInputException("Invalid input Infection Mode set to Random");
                }

            }
            catch (InvalidInputException e)
            {

                Console.WriteLine(e.msg);
                temp = 1;
            }

            return temp;
        }

        public ISimulation SimulationSelector() 
        {
            int simulationType = SimulationTypeSelect();

            int threadCount = 0;
            int isMapVisible = 0; 

            if (simulationType == 0)
            {
                threadCount = ThreadCountSelect();
                isMapVisible = VisualizeFlagSelect();

                return new SimulationReplay(threadCount, isMapVisible == 1);
            }
            else
            {
                int mapX = MapXSelect();
                int mapY = MapYSelect();
                int peopleCount = PeopleCountSelect();
                int infectedCountSelect = InfectedCountSelect(peopleCount);
                double infectionRate = InfectionRateSelect();
                double recoveryRate = RecoveryRateSelect();
                int mode = ZombieModeSelect(); 

                threadCount = ThreadCountSelect();
                isMapVisible = VisualizeFlagSelect();

                return new SimulationGenerate(threadCount,mapX,mapY,peopleCount,isMapVisible == 1, mode == 1, infectedCountSelect,infectionRate, recoveryRate);
            }
        }

        public bool StartAgain() {

            int x = Console.CursorLeft;
            int y = Console.CursorTop;
            Console.CursorTop = Console.WindowTop + Console.WindowHeight - 1;
            Console.Write("Simulate Again? (0 - no, 1 - yes)");

            string result = GetUserInput();

            int temp = -1;

            try
            {
                temp = ConvertToInt(result);
                if (temp < 0 || temp > 1)
                {
                    throw new InvalidInputException("Invalid input Simulation ends");
                }
            }
            catch (InvalidInputException e)
            {
                Console.WriteLine(e.msg);
                temp = 0;
            }

            return temp == 1;
        }

        public int ConvertToInt(string input) 
        {
            int temp = 0;
            try
            {
                temp = Convert.ToInt32(input);
            }
            catch (Exception e)
            {

                throw new InvalidInputException();
            }

            return temp;
        }

        public double ConvertToDouble(string input)
        {
            double temp = 0;
            try
            {
                temp = Convert.ToDouble(input);
            }
            catch (Exception e)
            {

                throw new InvalidInputException();
            }

            return temp;
        }
    }
}
