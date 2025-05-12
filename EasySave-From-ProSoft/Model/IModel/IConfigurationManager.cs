using EasySave_From_ProSoft.Model.ImplementIModel;
using System.Collections.Generic;

namespace EasySave_From_ProSoft.Model.IModel
{
    public interface IConfigurationManager
    {
        public List<BackupJob> LoadJobConfig();
        public void saveJobConfigs(List<BackupJob> config);
        public AppConfiguration LoadAppConfig();
        public void saveAppConfig(AppConfiguration config);
    }
}
