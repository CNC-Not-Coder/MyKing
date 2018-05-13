using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MainServer
{
    class LogModule
    {
        public delegate void LogDelegate(string format, params object[] arg);

        public static LogDelegate LogInfo = Console.WriteLine;
    }
}
