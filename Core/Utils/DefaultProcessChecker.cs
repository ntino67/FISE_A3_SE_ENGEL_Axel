using System.Diagnostics;
using Core.Model.Interfaces;

namespace Core.Utils
{
    public class DefaultProcessChecker : IProcessChecker
    {
        public bool IsProcessRunning(string processName)
        {
            return Process.GetProcessesByName(processName).Length > 0;
        }
    }
}