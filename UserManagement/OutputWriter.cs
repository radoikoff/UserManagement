using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserManagement
{
    public static class OutputWriter
    {
        public static void DisplayMessage(string message)
        {
            Console.WriteLine(message);
        }

        public static void WriteMessageInLogFile(string message)
        {
            //string logFileName = ConfigurationManager.AppSettings.Get("LogFileName");
            //File.AppendAllText(logFileName, DateTime.Now + " : " + message + "\r\n");
        }

        public static void DisplayMessageAndAddToLogFile(string message)
        {
            Console.WriteLine(message);
            //string logFileName = ConfigurationManager.AppSettings.Get("LogFileName");
            //File.AppendAllText(logFileName, DateTime.Now + " : " + message + "\r\n");
        }

        public static void DisplayException(string message)
        {
            ConsoleColor currentColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(message);
            Console.ForegroundColor = currentColor;
        }

    }
}
