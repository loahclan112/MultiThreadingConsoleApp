using System.Collections.Generic;
using System.IO;

namespace MultiThreadingConsoleApp
{
    public static class FileHandler
    {
        static string file = Directory.GetCurrentDirectory() + "\\Data.txt";
        static string fileGenerated = Directory.GetCurrentDirectory() + "\\DataGenerated.txt";

        public static List<string> ReadFromFile()
        {
            return new List<string>(File.ReadAllLines(fileGenerated));
        }

        public static List<string> ReadFromFile(string inputfile)
        {
            return new List<string>(File.ReadAllLines(inputfile));
        }

        public static void WriteToFile(List<string> outputContent, bool isGenerating)
        {
            string file = FileHandler.file;
            if (isGenerating)
            {
                file = FileHandler.fileGenerated;
            }
            File.WriteAllLines(file, outputContent);
        }
    }
}
