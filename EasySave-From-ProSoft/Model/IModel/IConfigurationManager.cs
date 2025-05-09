namespace EasySave_From_ProSoft.Interface.IModel
{
    public interface IConfigurationManager
    {
        public List<BackupJob> LoadJobConfig();
        public void saveJobConfigs(List<BackupJob> config);
        public AppConfiguration LoadAppConfig();
        public void saveAppConfig(AppConfiguration config);
    }
}
