using System;
using System.Collections.Generic;
using EasySave.Model.ImplementIModel;

namespace EasySave_From_ProSoft.Model.IModel
{
    public interface ILogger
    {
        public void Log(LogEntry entry);
        public List<LogEntry> LoadLog(DateTime date);
    }
}
