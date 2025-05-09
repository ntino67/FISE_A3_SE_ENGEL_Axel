namespace EasySave.Models
{
    public class BackupResult
    {
        public bool success { get; set; }
        public string message { get; set; }
        public TimeSpan duration { get; set; }
    }
}
