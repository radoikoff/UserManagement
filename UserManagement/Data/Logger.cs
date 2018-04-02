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
        public Logger()
        {
            this.Summary = new Summary();
        }

        public Summary Summary { get;}

        private void DisplayException(string message)
        {
            ConsoleColor currentColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(message);
            Console.ForegroundColor = currentColor;
        }

        private void DisplayMessageOnConsole(string message)
        {
            Console.WriteLine(message);
        }

        public void DisplayMessage(MsgType msgType, string message, params string[] args)
        {
            var resultMessage = string.Format(message, args);
            DisplayMessageOnConsole(resultMessage);
            WriteInLogFile(resultMessage);

            UpdateSummaryStat(msgType);
        }

        public void DisplaySummaryStats()
        {
            DisplayMessageOnConsole(this.Summary.ToString());
            WriteInLogFile(this.Summary.ToString());
        }

        private void UpdateSummaryStat(MsgType msgType)
        {

            switch (msgType)
            {
                case MsgType.INFO:
                    this.Summary.InfoMsgCount++;
                    break;
                case MsgType.ACTION:
                    this.Summary.ActionMsgCount++;
                    break;
                case MsgType.NOTASSIGNED:
                    this.Summary.NotAssignedMsgCount++;
                    break;
                case MsgType.WARNING:
                    this.Summary.WarningMsgCount++;
                    break;
                case MsgType.ERROR:
                    this.Summary.ErrorMsgCount++;
                    break;
                default:
                    break;
            }
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
