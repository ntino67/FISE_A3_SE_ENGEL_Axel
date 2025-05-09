namespace EasySave.Models
{
    public class BackupJob
    {
        public string idName { get; set; }
        public string displayName { get; set; }
        public string source { get; set; }
        public string target { get; set; }
        public BackupType Type { get; set; }
    }
}
