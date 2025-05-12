using System;
using System.Collections.Generic;
using System.IO;
using EasySave_From_ProSoft.Model.IModel;
using Newtonsoft.Json;
using EasySave_From_ProSoft.Model.ImplementIModel;

namespace EasySave_From_ProSoft.Model.ImplementIModel
{
    public sealed class Logger : ILogger
    {
        private static readonly object LockObj = new{};
        private static Logger _instance { get; set; }

        public string LogFilePath { get; set; } = "logs.json";

        private Logger() { }

        public static Logger Instance
        {
            get
            {
                lock (LockObj)
                {
                    _instance ??= new Logger();
                    return _instance;
                }
            }
        }

        public void Log(LogEntry entry)
        {
            List<LogEntry> logList = new List<LogEntry>();

            if (File.Exists(LogFilePath))
            {
                string existingJson = File.ReadAllText(LogFilePath);
                if (!string.IsNullOrWhiteSpace(existingJson))
                {
                    logList = JsonConvert.DeserializeObject<List<LogEntry>>(existingJson) ?? new List<LogEntry>();
                }
            }

            logList.Add(entry);

            string updatedJson = JsonConvert.SerializeObject(logList, Formatting.Indented);
            File.WriteAllText(LogFilePath, updatedJson);
        }
        
        public List<LogEntry> LoadLog(DateTime date)
        {
            throw new Exception("not implemented yet");
        }
    }
}
