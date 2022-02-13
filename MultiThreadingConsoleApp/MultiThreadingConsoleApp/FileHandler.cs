using System.Collections.Generic;
using System.IO;

namespace MultiThreadingConsoleApp
{
    public static class FileHandler
    {

        static string file = Directory.GetCurrentDirectory()+"\\Data.txt";


        public static List<string> ReadFromFile()
        {
            return new List<string>(File.ReadAllLines(file));
        }

        public static List<string> ReadFromFile(string inputfile)
        {

            return new List<string>(File.ReadAllLines(inputfile));
        }
        public static void WriteToFile(string outputContent) {

            File.WriteAllText(file, outputContent);
        }

        public static void WriteToFile(List<string> outputContent)
        {
            File.WriteAllLines(file, outputContent);
        }
    }
}
