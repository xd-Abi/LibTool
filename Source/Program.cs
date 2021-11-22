using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml;

namespace LibTool
{
    // Static Logger
    static class Log
    {
        static string s_CurrentStatus = "";

        /// <summary>
        /// Writes a empty line
        /// </summary>
        public static void WriteLine()
        {
            Console.WriteLine();
        }

        public static void WriteLine(string inFormat, params object[] inArgs)
        {
            Console.WriteLine(inFormat, inArgs);
        }

        public static void WriteError(string inFormat, params object[] inArgs)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(inFormat, inArgs);
            Console.ResetColor();
        }

        /// <summary>
        /// Overrides the old status 
        /// </summary>
        /// <param name="inStatus">The new status as string</param>
        public static void WriteStatus(string inStatus)
        {
            // if the status is larger than the console with, truncate it
            string NewStatus = inStatus;

            try
            {
                int Width = Console.BufferWidth;

                if (NewStatus.Length >= Width)
                {
                    NewStatus = NewStatus.Substring(0, Width - 1);
                }
            }
            catch (Exception)
            {
            }

            // Write the new status
            Console.Write("\r" + NewStatus);
            if (NewStatus.Length < s_CurrentStatus.Length)
            {
                Console.Write(new string(' ', s_CurrentStatus.Length - NewStatus.Length) + "\r" + NewStatus);
            }

            s_CurrentStatus = NewStatus;
        }

        /// <summary>
        /// Flushes the status and sets the current 
        /// status to ""
        /// </summary>
        public static void FlushStatus()
        {
            if (s_CurrentStatus.Length > 0)
            {
                Console.WriteLine();
                s_CurrentStatus = "";
            }
        }
    }

    // Config
    static class Config
    {
        /// <summary>
        /// Default is always the current directory (Directory.GetCurrentDirectory())
        /// </summary>
        public static string RootPath { get; set; } = Directory.GetCurrentDirectory();

        /// <summary>
        /// If set to true all Include commands needs to be realtive
        /// to RootPath
        /// </summary>
        public static bool DefaultInRoot { get; set; } = true;

        /// <summary>
        /// Overrides files if already exists
        /// </summary>
        public static bool Override { get; set; } = false;
    }

    // Main Program
    class Program
    {
        /// <summary>
        /// Main Entry point 
        /// </summary>
        /// <param name="inArgs">Command line parameters</param>
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
                Log.WriteError(e.Message);
            }
        }

        /// <summary>
        /// Parse all command line parameters and starts 
        /// the program
        /// </summary>
        /// <param name="inArgs">The arguments to parse</param>
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
            bool printHelp = ParseBool(argsList, "help", false);
            string file = ParseString(argsList, "file=", null);

            // Checking the arguments
            if (string.IsNullOrEmpty(file))
            {
                Log.WriteError("File was null or empty. Use {0}help", argsPrefix);
            }
            else
            {
                // The file will be checked after in ReadFile()

                file = ProcessPath(file, Directory.GetCurrentDirectory());
            }

            // Print error if there are any more arguments
            foreach (string remainingArg in argsList)
            {
                Log.WriteError("Invalid command line parameter: {0}", remainingArg);
                Log.WriteLine();

                printHelp = true;
            }

            // Printing help message
            if (printHelp)
            {
                PrintHelp();
            }

            Log.WriteLine("Checking...");
            ReadFile(file);
        }

        // Utility functions

        /// <summary>
        /// This function is only used to parse
        /// command line parameters.
        /// </summary>
        /// <param name="inArgsList">The args list</param>
        /// <param name="inName">The parameter name</param>
        /// <param name="inDefaultValue">A defazkt value</param>
        /// <returns>True if name was found in args list</returns>
        static bool ParseBool(List<string> inArgsList, string inName, bool inDefaultValue = false)
        {
            for (int i = 0; i < inArgsList.Count(); i++)
            {
                if (string.Compare(inArgsList[i], inName, true) == 0)
                {
                    inArgsList.RemoveAt(i);
                    return true;
                }
            }

            return inDefaultValue;
        }

        /// <summary>
        /// This function is only used to parse
        /// command line parameters.
        /// </summary>
        /// <param name="inArgsList">The args list</param>
        /// <param name="inName">The parameter name</param>
        /// <param name="inDefaultValue">A defazkt value</param>
        /// <returns>A substring from args list</returns>
        static string ParseString(List<string> inArgsList, string inName, string inDefaultValue = "")
        {
            for (int i = 0; i < inArgsList.Count(); i++)
            {
                if (inArgsList[i].StartsWith(inName))
                {
                    string output = inArgsList[i].Substring(inName.Length);
                    inArgsList.RemoveAt(i);

                    return output;
                }
            }

            return inDefaultValue;
        }

        static string ParseString(XmlNode inParentNode, string inName, string inDefaultValue = "")
        {
            for (int i = 0; i < inParentNode.ChildNodes.Count; i++)
            {
                if (CompareString(inParentNode.ChildNodes[i].Name, inName))
                {
                    return inParentNode.ChildNodes[i].InnerText;
                }
            }

            return inDefaultValue;
        }

        static bool ParseBool(XmlNode inParentNode, string inName, bool inDefaultValue = false)
        {
            for (int i = 0; i < inParentNode.ChildNodes.Count; i++)
            {
                if (CompareString(inParentNode.ChildNodes[i].Name, inName))
                {
                    return ParseBool(inParentNode.ChildNodes[i].InnerText);
                }
            }

            return inDefaultValue;
        }

        static bool ParseBool(string inValue)
        {
            // Parsing the value manually to avoid bool.Parse() exception

            bool output = false;

            if (string.IsNullOrEmpty(inValue))
            {
                throw new Exception("Config: Override was null or empty!");
            }

            if (string.Equals(inValue, "True", StringComparison.OrdinalIgnoreCase))
            {
                output = true;
            }
            else if (string.Equals(inValue, "False", StringComparison.OrdinalIgnoreCase))
            {
                output = false;
            }
            else
            {
                // Handling exceptions
                throw new Exception(string.Format("Config: Override was a invalid value! '{0}'", inValue));
            }

            return output;
        }

        static string ParseAttribute(XmlNode inNode, string inName, string inDefaultValue = "")
        {
            foreach (XmlAttribute attribute in inNode.Attributes)
            {
                if (CompareString(attribute.Name, inName))
                {
                    if (!string.IsNullOrEmpty(attribute.Value))
                    {
                        return attribute.Value;
                    }
                }
            }

            return inDefaultValue;
        }
        static bool ParseAttribute(XmlNode inNode, string inName, bool inDefaultValue)
        {
            foreach (XmlAttribute attribute in inNode.Attributes)
            {
                if (CompareString(attribute.Name, inName))
                {
                    if (!string.IsNullOrEmpty(attribute.Value))
                    {
                        return ParseBool(attribute.Value);
                    }
                }
            }

            return inDefaultValue;
        }

        /// <summary>
        /// Comapres two strings. 
        /// NOTE: StringComparison.OrdinalIgnoreCase
        /// </summary>
        /// <param name="inA">The first string to compare</param>
        /// <param name="inB">The second string to compare</param>
        /// <returns></returns>
        static bool CompareString(string inA, string inB)
        {
            return string.Equals(inA, inB, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Replaces '/' to system dir separator char
        /// and combines with a realtive path
        /// </summary>
        /// <param name="inPath">A path to process</param>
        /// <param name="inRelativeDir">A realative path to combine</param>
        /// <returns></returns>
        static string ProcessPath(string inPath, string inRelativeDir)
        {
            string path = inPath;
            path = inPath.Replace('/', Path.DirectorySeparatorChar);
            path = Path.Combine(inRelativeDir, path);

            return path;
        }

        /// <summary>
        /// Searches for files a directory
        /// </summary>
        /// <param name="inSearchDir">The search directory</param>
        /// <param name="inFilter">A filter. (**.xml)</param>
        /// <returns>A list of paths</returns>
        static List<string> SearchFiles(string inSearchDir, string inFilter)
        {
            if (!Directory.Exists(inSearchDir))
            {
                throw new Exception(string.Format("Directory was not found! '{0}'", inSearchDir));
            }

            List<string> output = new List<string>();

            List<string> directories = new List<string>(Directory.GetDirectories(inSearchDir, "*", SearchOption.AllDirectories));
            directories.Add(inSearchDir);

            directories.ForEach(delegate (string dir)
            {
                List<string> foundfiles = new List<string>(Directory.GetFiles(dir, inFilter));

                foundfiles.ForEach(delegate (string file)
                {
                    output.Add(file);
                });
            });

            return output;
        }

        // Core functions

        /// <summary>
        /// Prints a help message
        /// </summary>
        static void PrintHelp()
        { }


        /// <summary>
        /// Reads libtool files in xml format
        /// </summary>
        /// <param name="inFilePath">A file to read</param>
        static void ReadFile(string inFilePath)
        {
            // Check if file exists
            if (!File.Exists(inFilePath))
            {
                throw new Exception(string.Format("File was not found! '{0}'", inFilePath));
            }

            // Reading xml file
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.Load(inFilePath);

            // There must be a node called LibTool, otherwise this file will be ignored!
            if (xmlDocument.GetElementsByTagName("LibTool").Count == 0)
            {
                return;
            }

            foreach (XmlNode rootNodes in xmlDocument.GetElementsByTagName("LibTool"))
            {
                foreach (XmlNode node in rootNodes.ChildNodes)
                {
                    switch (node.Name)
                    {
                        case "Config": ProcessConfig(node, inFilePath); break;
                        case "Include": ProcessInclude(node, inFilePath); break;
                        // Any other node will be ignored!
                        default: break;
                    }
                }
            }
        }

        /// <summary>
        /// Process all nodes called 'Config'
        /// </summary>
        /// <param name="inConfigNode">The node to process</param>
        /// <param name="inFilePath">The xml file path</param>
        static void ProcessConfig(XmlNode inConfigNode, string inFilePath)
        {
            foreach (XmlNode childNode in inConfigNode.ChildNodes)
            {
                switch (childNode.Name)
                {
                    case "Override":
                        {
                            Config.Override = ParseBool(childNode.InnerText);
                        }
                        break;

                    case "RootPath":
                        {
                            string xmlFileDir = Path.GetDirectoryName(inFilePath);
                            string path = childNode.InnerText;
                            bool create = ParseAttribute(childNode, "Create", false);

                            if (string.IsNullOrEmpty(path))
                            {
                                throw new Exception("Config: RootPath was null or empty!");
                            }

                            path = ProcessPath(path, xmlFileDir);

                            // Check if path exists
                            if (!Directory.Exists(path) && !create)
                            {
                                throw new Exception("Config: RootPath was not found!");
                            }
                            else 
                            {
                                Directory.CreateDirectory(path);
                            }

                            Config.RootPath = path;
                        }
                        break;

                    case "DefaultInRoot":
                        {
                            Config.DefaultInRoot = ParseBool(childNode.InnerText);
                        }
                        break;
                }
            }
        }

        /// <summary>
        /// Process all nodes called 'Include'
        /// </summary>
        /// <param name="inIncludeNode">The node to process</param>
        /// <param name="inFilePath">The xml file path</param>
        static void ProcessInclude(XmlNode inIncludeNode, string inFilePath)
        {
;
            string rootDir = Path.GetDirectoryName(inFilePath);

            // Processing root path
            {
                bool inRoot = ParseAttribute(inIncludeNode, "InRoot", Config.DefaultInRoot);

                if (inRoot)
                {
                    rootDir = Config.RootPath;
                }
            }

            // Child nodes
            foreach (XmlNode childNode in inIncludeNode.ChildNodes)
            {
                switch (childNode.Name)
                {
                    case "File":
                        {
                            string path = childNode.InnerText;

                            // Check path 
                            if (string.IsNullOrEmpty(path))
                            {
                                throw new Exception("Include: Path was null or empty!");
                            }

                            path = ProcessPath(path, rootDir);

                            if (!File.Exists(path))
                            {
                                throw new Exception(string.Format("Include: File was not found! '{0}'", path));
                            }

                            // Include
                            ReadFile(path);
                        }
                        break;

                    case "Directory":
                        {
                            if (childNode.ChildNodes.Count == 0)
                            {
                                throw new Exception("Include: Directory has no child nodes!");
                            }

                            string path = ParseString(childNode, "Path", null);
                            string filter = ParseString(childNode, "Filter", null);

                            // Checking 
                            if (string.IsNullOrEmpty(path))
                            {
                                throw new Exception("Include: Directory path was null or empty!");
                            }

                            if (string.IsNullOrEmpty(filter))
                            {
                                throw new Exception("Include: Filter was null or empty");
                            }

                            path = ProcessPath(path, rootDir);

                            if (!Directory.Exists(path))
                            {
                                throw new Exception("Include: Directory not found!");
                            }


                            // Searching files and including
                            List<string> foundFiles = SearchFiles(path, filter);

                            foreach (string file in foundFiles)
                            {
                                ReadFile(file);
                            }
                            
                        }
                        break;
                }
            }
        }
    }
}
