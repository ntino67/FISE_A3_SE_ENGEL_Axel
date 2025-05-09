namespace EasySave.Interfaces
{
    public interface IConfigurationManager
    {
        public List<BackupJob> LoadJobConfig();
        public void saveJobConfigs(List<BackupJob> config);
        public AppConfiguration LoadAppConfig();
        public void saveAppConfig(AppConfiguration config);
    }
}
