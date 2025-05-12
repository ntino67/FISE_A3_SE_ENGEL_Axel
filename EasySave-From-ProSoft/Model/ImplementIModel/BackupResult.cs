using System;

namespace EasySave_From_ProSoft.Model.ImplementIModel
{
    public class BackupResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public TimeSpan Duration { get; set; }
    }
}
