using EasySave_From_ProSoft.Model.ImplementIModel;
using System.Collections.Generic;

namespace EasySave_From_ProSoft.Model.IModel
{
    public interface IConfigurationManager
    {
        public List<BackupJob> LoadJobConfig();
        public void SaveJobConfigs(List<BackupJob> config);
        public AppConfiguration LoadAppConfig();
        public void SaveAppConfig(AppConfiguration config);
    }
}
