using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiThreadingConsoleApp
{

    class UIConsole
    {
        public string GetUserInput() {
            return Console.ReadLine();
        }

        private int InfectedCountSelect()
        {
            Console.Write("Select Infected Person Count (Suggested 1 - peopleCount):  ");
            string result = GetUserInput();

            int temp = -1;

            try
            {
                temp = ConvertToInt(result);
                if (temp < 1 || temp > 1000)
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
            Console.Write("Select Simulation type (0 - Normal, 1 - Random):  ");
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
            Console.Write("Select Population Count (Suggested: 1 - 100):  ");
            string result = GetUserInput();

            int temp = -1;

            try
            {
                temp = ConvertToInt(result);
                if (temp < 1 || temp > 1000)
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

        private int ThreadCountSelect()
        {
            Console.Write("Select Thread Count (1 - 6):  ");
            string result = GetUserInput();

            int temp = -1;

            try
            {
                temp = ConvertToInt(result);
                if (temp < 1 || temp > 6)
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
                    throw new Exception("Invalid input Visualization turned ON");
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

            Console.Write("Select Map X (Suggested: 10 - 150):  ");
            string result = GetUserInput();

            int temp = -1;

            try
            {
                temp = ConvertToInt(result);
                if (temp < 5 || temp > 200)
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

            Console.Write("Select Map Y (Suggested: 10 - 150):  ");
            string result = GetUserInput();

            int temp = -1;

            try
            {
                temp = ConvertToInt(result);
                if (temp < 5 || temp > 200)
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
                    throw new InvalidInputException("Invalid input Infection Mode turned ON");
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

                return new Simulation(threadCount, isMapVisible == 1);
            }
            else
            {
                int mapX = MapXSelect();
                int mapY = MapYSelect();
                int peopleCount = PeopleCountSelect();
                int infectedCountSelect = InfectedCountSelect();
                int mode = ZombieModeSelect(); 

                threadCount = ThreadCountSelect();
                isMapVisible = VisualizeFlagSelect();

                return new SimulationRandom(threadCount,mapX,mapY,peopleCount,isMapVisible == 1, mode == 1, infectedCountSelect);

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
    }
}
