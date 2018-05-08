namespace UserManagement.IO
{
    using System;
    using System.Collections.Generic;
    using StaticData;

    public class TestLogger : ILogger
    {
        private List<string> resultMessages;

        public TestLogger()
        {
            this.resultMessages = new List<string>();
        }

        public void DisplayMessage(MsgType msgType, string message, params string[] args)
        {
            this.resultMessages.Add(msgType.ToString() + ": " + string.Format(message, args));
        }

        public void DisplaySummaryStats()
        {
            throw new NotImplementedException();
        }
    }
}
