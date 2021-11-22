using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml;

namespace LibTool
{
    class Program
    {
        public class ProgramException : Exception
        {
            public ProgramException(string inMessage, params object[] inArgs)
                : base(string.Format(inMessage, inArgs))
            { }
        }

        static void Main(string[] inArgs)
        {
            try
            {
                Start(inArgs);
            }
            catch (Exception e)
            {
                Log.FlushStatus();
                Log.WriteError("An Error has occurred:");
                Log.WriteError(e.ToString());
            }
        }

        static void Start(string[] inArgs)
        {
            List<string> argsList = new List<string>(inArgs);

            // Removing the prefix of arguments
            string argsPrefix = "--";

            for (int i = 0; i < argsList.Count(); i++)
            {
                if (argsList[i].StartsWith(argsPrefix))
                {
                    argsList[i] = argsList[i].Substring(argsPrefix.Length);
                }
            }

            // Parse the arguments
            bool printHelp = Utils.ParseBool(argsList, "help", false);
            string file = Utils.ParseString(argsList, "file=", null);

            // Checking arguments

            if (printHelp)
            {
                PrintHelp();
                return;
            }

            // if there are any more arguments, print an error
            foreach (string remainingArg in argsList)
            {
                Log.WriteError("Invalid command line parameter: {0}", remainingArg);
                Log.WriteError("Use --help.");
                Log.WriteLine();

                return;
            }

            if (string.IsNullOrEmpty(file))
            {
                throw new ProgramException("File was null or empty! Use --help.");
            }

            // The file will be checked after in ReadFile().
            file = Utils.ProcessPath(file, Directory.GetCurrentDirectory());

            Log.WriteLine("Checking...");
            ReadFile(file);
        }

        static void PrintHelp()
        {
            // Header
            Log.WriteLine();
            Log.SetColor(ConsoleColor.Cyan);
            Log.WriteLine("====================== LibTool ======================");
            Log.ResetColor();

            Log.WriteLine();
            Log.WriteLine(" Description:");
            Log.WriteLine("    This tool downloads dependencies from the");
            Log.WriteLine("    internet.");
            Log.WriteLine();
            Log.WriteLine(" Usage:");
            Log.WriteLine("    LibTool [options]");
            Log.WriteLine();
            Log.WriteLine(" Options:");
            Log.WriteLine("    --file=          A xml file to start the program");
            Log.WriteLine();

            // Foot
            Log.SetColor(ConsoleColor.Cyan);
            Log.WriteLine("=====================================================");
            Log.ResetColor();
            Log.WriteLine();
        }

        static void ReadFile(string inFilePath)
        {
            // Check if file exsits
            if (!File.Exists(inFilePath))
            {
                throw new ProgramException("File not found! ({0})", inFilePath);
            }

            // Reading xml file
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.Load(inFilePath);

            // There must be a node called LibTool otherwise this file will be ignored!
            foreach (XmlNode rootNode in xmlDocument.GetElementsByTagName("LibTool"))
            {
                foreach (XmlNode childNode in rootNode.ChildNodes)
                {
                    string name = childNode.Name;

                    if (Utils.Compare(name, "Config"))
                    {
                        ProcessConfig(childNode, inFilePath);
                    }
                    else if (Utils.Compare(name, "Include"))
                    {
                        ProcessInclude(childNode, inFilePath);
                    }
                    else if (Utils.Compare(name, "Library"))
                    {
                        ProcessLibrary(childNode, inFilePath);
                    }
                    else
                    {
                        Log.WriteWarning("{0}: Unknown node({1})!", Path.GetFileName(inFilePath), name);
                    }
                }
            }
        }

        static void ProcessConfig(XmlNode inConfigNode, string inFilePath)
        {
            foreach (XmlNode node in inConfigNode.ChildNodes)
            {
                if (Utils.Compare(node.Name, "Override"))
                {
                    if (string.IsNullOrEmpty(node.InnerText))
                    {
                        throw new ProgramException("{0}: Override was null or empty! Valid options: 'True', 'False'.", Path.GetFileName(inFilePath));
                    }

                    Config.Override = bool.Parse(node.InnerText);
                }
                else if (Utils.Compare(node.Name, "DefaultInRoot"))
                {
                    if (string.IsNullOrEmpty(node.InnerText))
                    {
                        throw new ProgramException("{0}: DefaultInRoot was null or empty! Valid options: 'True', 'False'.", Path.GetFileName(inFilePath));
                    }

                    Config.DefaultInRoot = bool.Parse(node.InnerText);
                }
                else if (Utils.Compare(node.Name, "RelativePath"))
                {
                    if (string.IsNullOrEmpty(node.InnerText))
                    {
                        throw new ProgramException("{0}: RelativePath was null or empty! Vaild options: 'Root', 'File'.", Path.GetFileName(inFilePath));
                    }

                    if (Utils.Compare(node.InnerText, "Root"))
                    {
                        Config.RelativePath = Config.RelativePathOptions.Root;
                    }
                    else if (Utils.Compare(node.InnerText, "File"))
                    {
                        Config.RelativePath = Config.RelativePathOptions.File;
                    }
                    else
                    {
                        throw new ProgramException("{0}: Invalid RelativePath! Vaild options: 'Root', 'File'.", Path.GetFileName(inFilePath));
                    }
                }
                else if (Utils.Compare(node.Name, "RootPath"))
                {
                    string path = node.InnerText;

                    if (string.IsNullOrEmpty(path))
                    {
                        throw new ProgramException("{0}: RootPath was null or empty! Use a relative path!", Path.GetFileName(inFilePath));
                    }

                    path = Utils.ProcessPath(path, Path.GetDirectoryName(inFilePath));

                    bool create = Utils.ParseAttribute(node, "Create", false);

                    if (!Directory.Exists(path) && !create)
                    {
                        throw new ProgramException("{0}: RootPath was not found! ({1})", Path.GetFileName(inFilePath), path);
                    }
                    else
                    {
                        Directory.CreateDirectory(path);
                    }

                    Config.RootPath = path;
                }
                else
                {
                    Log.WriteWarning("{0}: Unknown node({1})!", Path.GetFileName(inFilePath), node.Name);
                }
            }
        }

        static void ProcessInclude(XmlNode inIncludeNode, string inFilePath)
        {
            foreach (XmlNode node in inIncludeNode.ChildNodes)
            {
                if (Utils.Compare(node.Name, "File"))
                {
                    string path = node.InnerText;
                    bool inRoot = Utils.ParseAttribute(node, "InRoot", Config.DefaultInRoot);

                    if (string.IsNullOrEmpty(path))
                    {
                        throw new ProgramException("{0}: Include-File: File was null or empty!", Path.GetFileName(inFilePath));
                    }

                    if (inRoot)
                    {
                        path = Utils.ProcessPath(path, Config.RootPath);
                    }
                    else
                    {
                        path = Utils.ProcessPath(path, Path.GetDirectoryName(inFilePath));
                    }

                    if (!File.Exists(path))
                    {
                        throw new ProgramException("{0}: Include-File: File was not found! ({1})", Path.GetFileName(inFilePath), path);
                    }

                    if (Path.Equals(path, inFilePath))
                    {
                        throw new ProgramException("{0}: Include-File: Includes it self!", Path.GetFileName(inFilePath));
                    }

                    ReadFile(path);
                }
                else if (Utils.Compare(node.Name, "Dir"))
                {
                    string path = Utils.ParseString(node, "Path", "");
                    string filter = Utils.ParseString(node, "Filter", null);
                    bool inRoot = Utils.ParseAttribute(node, "InRoot", Config.DefaultInRoot);

                    // Check path

                    if (string.IsNullOrEmpty(path))
                    {
                        throw new ProgramException("{0}: Include-Dir: Path was null or empty!", Path.GetFileName(inFilePath));
                    }

                    if (inRoot)
                    {
                        path = Utils.ProcessPath(path, Config.RootPath);
                    }
                    else
                    {
                        path = Utils.ProcessPath(path, Path.GetDirectoryName(inFilePath));
                    }

                    if (!Directory.Exists(path))
                    {
                        throw new ProgramException("{0}: Include-Dir: Path was not found! ({1})", Path.GetFileName(inFilePath), path);
                    }

                    // Check filter

                    if (string.IsNullOrEmpty(filter))
                    {
                        throw new ProgramException("{0}: Include-Dir: Filter was null or empty!", Path.GetFileName(inFilePath));
                    }

                    // Searching and including file 

                    List<string> foundFiles = Utils.SearchFiles(path, filter);

                    foreach (string file in foundFiles)
                    {
                        if (Path.Equals(file, inFilePath))
                        {
                            throw new ProgramException("{0}: Include-Dir includes it self!", Path.GetFileName(inFilePath));
                        }

                        ReadFile(file);
                    }
                }
                else
                {
                    Log.WriteWarning("{0}: Unknown node({1})!", Path.GetFileName(inFilePath), node.Name);
                }

            }
        }

        static void ProcessLibrary(XmlNode inIncludeLibrary, string inFilePath)
        {

        }
    }
}
