using System;

namespace EasySave_From_ProSoft.Model.ImplementIModel
{
    public class BackupResult
    {
        public bool success { get; set; }
        public string message { get; set; }
        public TimeSpan duration { get; set; }
    }
}
