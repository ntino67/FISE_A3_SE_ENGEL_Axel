namespace EasySave_From_ProSoft.Model.ImplementIModel
{
    public class BackupJob
    {
        public string IdName { get; set; }
        public string DisplayName { get; set; }
        public string Source { get; set; }
        public string Target { get; set; }
        public BackupType Type { get; set; }
    }
}
