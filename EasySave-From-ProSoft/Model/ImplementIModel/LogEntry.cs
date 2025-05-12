using System;

namespace EasySave_From_ProSoft.Model.ImplementIModel
{
    public class LogEntry
    {
        public DateTime Timestamp { get; set; }
        public string JobName { set; get; }
        public string SourcePath { set; get; }
        public string TargetPath { set; get; }
        public long FileSize { set; get; }
        public long TransferTime { set; get; }
    }
}
