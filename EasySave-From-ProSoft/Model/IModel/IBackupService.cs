using EasySave.Model.ImplementIModel;
using System.Collections.Generic;

namespace EasySave.Interface.IModel
{
    public interface IBackupService
    {
        public BackupResult ExecuteBackupJob(string idName);
        public List<BackupResult> ExecuteAllBackupJobs();
        public void AddBackupJob(BackupJob job);
        public void DeleteBackuJob(string idName);
        public List<BackupJob> GetAllJobs();
    }
}
