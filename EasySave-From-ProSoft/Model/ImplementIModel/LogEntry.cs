using System;

namespace EasySave.Model.ImplementIModel
{
    public class LogEntry
    {
        public DateTime timestamp { get; set; }
        public string jobName { set; get; }
        public string sourcePath { set; get; }
        public string targetPath { set; get; }
        public long fileSize { set; get; }
        public long transferTime { set; get; }
    }
}
