using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserManagement.Data
{
    class Logger
    {
        public void DisplayException(string message)
        {
            ConsoleColor currentColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(message);
            Console.ForegroundColor = currentColor;
        }

        public void DisplayMessage(string message, params string[] args)
        {
            var resultMessage = string.Format(message, args);
            Console.WriteLine(resultMessage);
            //string logFileName = ConfigurationManager.AppSettings.Get("LogFileName");
            //File.AppendAllText(logFileName, DateTime.Now + " : " + message + "\r\n");
        }
    }
}
