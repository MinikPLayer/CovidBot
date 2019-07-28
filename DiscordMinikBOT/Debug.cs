using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordMinikBOT
{
    public static class Debug
    {

        const string engineLogPrefix = "[ENGINE] ";
        private static void _Log(object data, ConsoleColor color, bool engineCall, ConsoleColor defaultColor = ConsoleColor.White)
        {
            Console.ForegroundColor = color;
            if(engineCall)
            {
                Console.Write(engineLogPrefix);
            }
            Console.WriteLine(data);
            Console.ForegroundColor = defaultColor;
        }


        static ConsoleColor normalColor = ConsoleColor.White;
        public static void Log(object data, bool engineCall = false)
        {
            _Log(data, normalColor, engineCall);
        }

        static ConsoleColor warningColor = ConsoleColor.DarkYellow;
        public static void LogWarning(object data, bool engineCall = false)
        {
            _Log(data, warningColor, engineCall);
        }

        static ConsoleColor errorColor = ConsoleColor.Red;
        public static void LogError(object data, bool engineCall = false)
        {
            _Log(data, errorColor, engineCall);
        }

        public static void EngineLog(object data)
        {
            Log(data, true);
        }

        public static void EngineLogWarning(object data)
        {
            LogWarning(data, true);
        }

        public static void EngineLogError(object data)
        {
            LogError(data, true);
        }


    }
}
