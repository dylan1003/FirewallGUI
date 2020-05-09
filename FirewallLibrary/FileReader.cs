using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace FirewallLibrary
{
    public static class FileReader
    {
        //Read all lines from an input file
        static List<string> ReadFile(string inputFile)
        {
            string inputData;
            try
            {
                using (StreamReader reader = new StreamReader(inputFile))
                {
                    reader.ReadLine();
                    inputData = reader.ReadToEnd();
                }
                return OrganiseInputString(inputData);
            }
            catch (FileNotFoundException fnfException)
            {
                Console.WriteLine(fnfException.Message + ": Could not find " + inputFile);
                return null;
            }
        }
        //Strip quotes and split data into rows
        static List<string> OrganiseInputString(string inputData)
        {
            inputData = inputData.Remove('"');
            return inputData.Split('\n').ToList();
        }

        static void ImportFiles(string[] files)
        {
            for (int i = 0; i < files.Length; i++)
            {
                ReadFile(files[i]);
            }
        }
    }
}
