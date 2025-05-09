namespace EasySave_From_ProSoft.Model.ImplementIModel
{
    public class BackupManager {
        private List<BackupManager> jobs { get; set; }
        private ILogger logger { get; set; }
        private IConfigurationManager config { get; set; }
    }
}
