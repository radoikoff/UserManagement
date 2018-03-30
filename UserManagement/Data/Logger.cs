using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
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
            WriteInLogFile(resultMessage);
        }

        private void WriteInLogFile(string message)
        {
            const string DATE_FORMAT = "yyyy-MM-dd HH:mm:ss:fff";

            string logFilePath = Path.Combine(ConfigurationManager.AppSettings.Get("DataFolderName"), ConfigurationManager.AppSettings.Get("LogFileName"));

            string formattedTimeStamp = DateTime.Now.ToString(DATE_FORMAT, CultureInfo.InvariantCulture);

            File.AppendAllText(logFilePath, formattedTimeStamp + " " + message + Environment.NewLine);
        }
    }
}
