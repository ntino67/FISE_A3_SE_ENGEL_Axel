namespace EasySave.Interface.IModel
{
    public interface ILogger
    {
        public void Log(LogEntry entry);
        public List<LogEntry> LoadLog(DateTime date);
    }
}
