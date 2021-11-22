using System;

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
            SetColor(ConsoleColor.Red);
            WriteLine(inFormat, inArgs);
            ResetColor();
        }

        public static void WriteWarning(string inFormat, params object[] inArgs)
        {
            SetColor(ConsoleColor.DarkYellow);
            WriteLine(inFormat, inArgs);
            ResetColor();
        }

        public static void SetColor(ConsoleColor inColor)
        {
            Console.ForegroundColor = inColor;
        }

        public static void ResetColor()
        {
            Console.ResetColor();
        }

        public static void WriteStatus(string inStatus, params object[] inArgs)
        {
            // if the status is larger than the console with, truncate it
            string NewStatus = string.Format(inStatus, inArgs);

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
}
