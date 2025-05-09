using EasySave.Model.ImplementIModel;
using EasySave.Model;
using System;
using System.Collections.Generic;

namespace EasySave.Interfaces
{
    public interface ILogger
    {
        public void Log(LogEntry entry);
        public List<LogEntry> LoadLog(DateTime date);
    }
}
