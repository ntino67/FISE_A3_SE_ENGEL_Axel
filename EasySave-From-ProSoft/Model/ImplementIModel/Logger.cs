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
            string logFilePath = GetLogFilePath(entry.Timestamp);
            
            List<LogEntry> logList = new List<LogEntry>();

            if (File.Exists(logFilePath))
            {
                string existingJson = File.ReadAllText(logFilePath);
                if (!string.IsNullOrWhiteSpace(existingJson))
                {
                    logList = JsonConvert.DeserializeObject<List<LogEntry>>(existingJson) ?? new List<LogEntry>();
                }
            }

            logList.Add(entry);

            string updatedJson = JsonConvert.SerializeObject(logList, Formatting.Indented);
            File.WriteAllText(logFilePath, updatedJson);
        }
        
        public List<LogEntry> LoadLog(DateTime date)
        {
            string logPath = $"logs/log-{date:yyyy-MM-dd}.json";
            
            if (!File.Exists(logPath))
            {
                return new List<LogEntry>();
            }

            string json = File.ReadAllText(logPath);
            var allLogs = JsonConvert.DeserializeObject<List<LogEntry>>(json) ?? new List<LogEntry>();
            
            return allLogs;
        }
        
        private string GetLogFilePath(DateTime date)
        {
            string folder = "logs";
            Directory.CreateDirectory(folder);
            return Path.Combine(folder, $"log-{date:yyyy-MM-dd}.json");
        }
    }
}
