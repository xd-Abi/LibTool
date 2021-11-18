using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;

namespace LibTool
{
    class Program
    {
        static void Main(string[] inArgs)
        {
            List<string> argsList = new List<string>(inArgs);

            // Removing the prefix
            NormalizeArgs(argsList, "--");

            // Parse the arguments
            bool printHelp = ParseBool(argsList, "help", false);
            string file = ParseString(argsList, "file=", "");

            // Checking the arguments
            if (string.IsNullOrEmpty(file))
            {
                Log.WriteError("File was null or empty. Use --help!");
            }

            // Print error if there are any more arguments
            foreach (string remainingArg in argsList)
            {
                Log.WriteError("Invalid command line parameter: {0}", remainingArg);
                Log.WriteLine();

                printHelp = true;
            }

            // Print help message
            if (printHelp)
            {
                PrintHelp();
            }

            // Start program
            try
            {
                Start(file);
            }
            catch (Exception e)
            {
                Log.WriteError(e.Message);
            }
        }

        static void NormalizeArgs(List<string> inArgsList, string inArgsPrefix)
        {
            for (int i = 0; i < inArgsList.Count(); i++)
            {
                if (inArgsList[i].StartsWith(inArgsPrefix))
                {
                    inArgsList[i] = inArgsPrefix.Substring(inArgsPrefix.Length);
                }
            }
        }

        static bool ParseBool(List<string> inArgsList, string inName, bool inDefaultValue = false)
        {
            for (int i = 0; i < inArgsList.Count(); i++)
            {
                if (String.Compare(inArgsList[i], inName, true) == 0)
                {
                    inArgsList.RemoveAt(i);
                    return true;
                }
            }

            return inDefaultValue;
        }

        static string ParseString(List<string> inArgsList, string inName, string inDefaultValue = "")
        {
            for (int i = 0; i < inArgsList.Count(); i++)
            {
                if (inArgsList[i].StartsWith(inName))
                {
                    String output = inArgsList[i].Substring(inName.Length);
                    inArgsList.RemoveAt(i);

                    return output;
                }
            }

            return inDefaultValue;
        }

        static void PrintHelp()
        {
            Log.WriteLine();
            Log.WriteLine("Help:");
            Log.WriteLine();
            Log.WriteLine("   Description:");
            Log.WriteLine("      LibTool searches for custom files in a given search");
            Log.WriteLine("      directory and downloads Libraries from the internet.");
            Log.WriteLine();
            Log.WriteLine("   Usage:");
            Log.WriteLine("      LibTool [Options]");
            Log.WriteLine();
            Log.WriteLine("   Options:");
            Log.WriteLine("      file=<PATH>");
            Log.WriteLine();
        }
    
        static void Start(string inFile)
        {
            string rootFile = ProcessPath(inFile, Directory.GetCurrentDirectory());
            
            if (!File.Exists(rootFile))
            {
                throw new Exception("File was not found! '" + rootFile + "'");
            }

            // TODO: Including files
            List<string> files = new List<string>();
            files.Add(rootFile);

            foreach (string file in files)
            {
                ProcessFile(file);
            }
        }

        static string ProcessPath(string inPath, string inRelativeDirPath = null)
        {
            string path = inPath;

            if (string.IsNullOrEmpty(inRelativeDirPath))
            {
                inRelativeDirPath = Directory.GetCurrentDirectory();
            }

            if (path.StartsWith("./"))
            {
                path = path.Substring(2);
                path = Path.Combine(inRelativeDirPath, path);
            }

            if (path.StartsWith("../"))
            {
                path = path.Replace('/', Path.DirectorySeparatorChar);
                path = new FileInfo(Path.Combine(inRelativeDirPath, path)).FullName;
            }

            path = path.Replace('/', Path.DirectorySeparatorChar);

            return path;
        }

        static void ProcessFile(string inFile)
        {

        }
    }
}
