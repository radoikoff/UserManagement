namespace UserManagement.IO
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using StaticData;

    public class Logger : ILogger
    {
        private int infoMsgCount;
        private int actionMsgCount;
        private int warningMsgCount;
        private int errorMsgCount;
        private int notAssignedMsgCount;

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
            var resultMessage = msgType.ToString() + ": " + string.Format(message, args);
            DisplayMessageOnConsole(resultMessage);
            WriteInLogFile(resultMessage);

            UpdateSummaryStat(msgType);
        }

        public void DisplaySummaryStats()
        {
            string summary = $"SUMMARY:: ERRORS:{this.errorMsgCount} WARNINGS:{this.warningMsgCount} ACTIONS:{this.actionMsgCount} INFOS:{this.infoMsgCount} NOT ASSIGNED:{this.notAssignedMsgCount}";
            DisplayMessageOnConsole(summary);
            WriteInLogFile(summary);
        }

        private void UpdateSummaryStat(MsgType msgType)
        {

            switch (msgType)
            {
                case MsgType.INFO:
                    this.infoMsgCount++;
                    break;
                case MsgType.ACTION:
                    this.actionMsgCount++;
                    break;
                case MsgType.NOTASSIGNED:
                    this.notAssignedMsgCount++;
                    break;
                case MsgType.WARNING:
                    this.warningMsgCount++;
                    break;
                case MsgType.ERROR:
                    this.errorMsgCount++;
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
