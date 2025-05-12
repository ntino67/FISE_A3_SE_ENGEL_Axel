using System;
using System.Collections.Generic;
using System.IO;
using EasySave_From_ProSoft.Model.ImplementIModel;

namespace EasySave_From_ProSoft.Model.IModel
{
    public interface ILogger
    {
        public void Log(LogEntry entry);
        public List<LogEntry> LoadLog(DateTime date);
        private string GetLogFilePath(DateTime date)
        {
            string folder = "logs";
            Directory.CreateDirectory(folder);
            return Path.Combine(folder, $"log-{date:yyyy-MM-dd}.json");
        }
    }
}
