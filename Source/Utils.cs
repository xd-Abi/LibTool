using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml;

namespace LibTool
{
    class Utils
    {
     
        // Argument Parser

        public static bool ParseBool(List<string> inArgsList, string inName, bool inDefaultValue = false)
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

        public static string ParseString(List<string> inArgsList, string inName, string inDefaultValue = "")
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

        // Xml Parser

        public static string ParseString(XmlNode inParentNode, string inName, string inDefaultValue = "")
        {
            for (int i = 0; i < inParentNode.ChildNodes.Count; i++)
            {
                if (Compare(inParentNode.ChildNodes[i].Name, inName))
                {
                    return inParentNode.ChildNodes[i].InnerText;
                }
            }

            return inDefaultValue;
        }

        public static bool ParseBool(XmlNode inParentNode, string inName, bool inDefaultValue = false)
        {
            for (int i = 0; i < inParentNode.ChildNodes.Count; i++)
            {
                if (Compare(inParentNode.ChildNodes[i].Name, inName))
                {
                    return bool.Parse(inParentNode.ChildNodes[i].InnerText);
                }
            }

            return inDefaultValue;
        }

        public static string ParseAttribute(XmlNode inNode, string inName, string inDefaultValue = "")
        {
            foreach (XmlAttribute attribute in inNode.Attributes)
            {
                if (Compare(attribute.Name, inName))
                {
                    if (!string.IsNullOrEmpty(attribute.Value))
                    {
                        return attribute.Value;
                    }
                }
            }

            return inDefaultValue;
        }
        public static bool ParseAttribute(XmlNode inNode, string inName, bool inDefaultValue)
        {
            foreach (XmlAttribute attribute in inNode.Attributes)
            {
                if (Compare(attribute.Name, inName))
                {
                    if (!string.IsNullOrEmpty(attribute.Value))
                    {
                        return bool.Parse(attribute.Value);
                    }
                }
            }

            return inDefaultValue;
        }

        public static string ProcessPath(string inPath, string inRelativeDir)
        {
            string path = inPath;
            path = inPath.Replace('/', Path.DirectorySeparatorChar);
            path = Path.Combine(inRelativeDir, path);

            return path;
        }


        public static bool Compare(string inA, string inB)
        {
            return string.Equals(inA, inB, StringComparison.OrdinalIgnoreCase);
        }

        public static List<string> SearchFiles(string inSearchDir, string inFilter)
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
    }
}
