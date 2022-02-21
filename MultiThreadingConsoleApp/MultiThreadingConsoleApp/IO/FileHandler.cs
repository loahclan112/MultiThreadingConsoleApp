using System.Collections.Generic;
using System.IO;

namespace MultiThreadingConsoleApp
{
    public static class FileHandler
    {
        static string file = Directory.GetCurrentDirectory() + "\\Data.txt";
        static string fileExcel = Directory.GetCurrentDirectory() + "\\DataExcel.txt";
        static string fileGenerated = Directory.GetCurrentDirectory() + "\\DataGenerated.txt";

        public static List<string> ReadFromFile()
        {
            try
            {
                return new List<string>(File.ReadAllLines(fileGenerated));

            }
            catch (IOException)
            {
                System.Console.WriteLine("No files yet Generated to Replay! Please Select Generate option first!");
                return new List<string>();
                
            }

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

        public static void WriteToFileExcel(List<string> outputContent)
        {
            string file = FileHandler.fileExcel;

            File.WriteAllLines(file, outputContent);
        }
    }
}
