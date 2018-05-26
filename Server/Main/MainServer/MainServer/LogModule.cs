using System;

namespace MyNetwork
{
    class LogModule
    {
        public delegate void LogDelegate(string format, params object[] arg);

        public static LogDelegate LogInfo = Console.WriteLine;

        public static LogDelegate LogError = Console.WriteLine;
    }
}
