using System;
using System.Collections.Generic;
using EasySave_From_ProSoft.Model.ImplementIModel;

namespace EasySave_From_ProSoft.Model.IModel
{
    public interface ILogger
    {
        public void Log(LogEntry entry);
        public List<LogEntry> LoadLog(DateTime date);
    }
}
