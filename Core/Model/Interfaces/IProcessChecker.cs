namespace Core.Model.Interfaces
{
    public interface IProcessChecker
    {
        bool IsProcessRunning(string processName);
    }
}