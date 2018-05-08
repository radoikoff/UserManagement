namespace UserManagement.IO
{
    using StaticData;

    public interface ILogger
    {
        void DisplayMessage(MsgType msgType, string message, params string[] args);
        void DisplaySummaryStats();
    }
}