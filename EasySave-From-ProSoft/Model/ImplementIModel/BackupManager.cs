using EasySave_From_ProSoft.Model.IModel;
using System.Collections.Generic;

namespace EasySave_From_ProSoft.Model.ImplementIModel
{
    public class BackupManager {
        private List<BackupManager> Jobs { get; set; }
        private ILogger Logger { get; set; }
        private IConfigurationManager Config { get; set; }
    }
}
