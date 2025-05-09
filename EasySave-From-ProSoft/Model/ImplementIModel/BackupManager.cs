using EasySave.Interface.IModel;
using EasySave.Interfaces;
using System.Collections.Generic;

namespace EasySave.Model.ImplementIModel
{
    public class BackupManager {
        private List<BackupManager> jobs { get; set; }
        private ILogger logger { get; set; }
        private IConfigurationManager config { get; set; }
    }
}
