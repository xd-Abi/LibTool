using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml;

namespace LibTool
{
    static class Log
    {
        static string s_CurrentStatus = "";

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

        public static void FlushStatus()
        {
            if (s_CurrentStatus.Length > 0)
            {
                Console.WriteLine();
                s_CurrentStatus = "";
            }
        }
    }

    static class Config
    {
        public static string RootPath { get; set; } = Directory.GetCurrentDirectory();

        public static bool DefaultInRoot { get; set; } = true;

        public static bool Override { get; set; } = false;
    }

    class Program
    {
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

            }

            Log.WriteLine("Checking...");
            ReadFile(file);
        }

        // Utility functions

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
                if (ComapreString(inParentNode.ChildNodes[i].Name, inName))
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
                if (ComapreString(inParentNode.ChildNodes[i].Name, inName))
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
                if (ComapreString(attribute.Name, inName))
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
                if (ComapreString(attribute.Name, inName))
                {
                    if (!string.IsNullOrEmpty(attribute.Value))
                    {
                        return ParseBool(attribute.Value);
                    }
                }
            }

            return inDefaultValue;
        }

        static bool ComapreString(string inA, string inB)
        {
            return string.Equals(inA, inB, StringComparison.OrdinalIgnoreCase);
        }

        static string ProcessPath(string inPath, string inRelativeDir)
        {
            string path = inPath;
            path = inPath.Replace('/', Path.DirectorySeparatorChar);
            path = Path.Combine(inRelativeDir, path);

            return path;
        }

        static List<string> SearchFiles(string inSearchDir, string inFilter)
        {
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
