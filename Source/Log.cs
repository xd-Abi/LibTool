using System;

namespace LibTool
{
    static class Log
    {
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
    }
}
