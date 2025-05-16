using System.Collections.Generic;
using System.Threading.Tasks;
using EasySave_From_ProSoft.Model;

namespace EasySave_From_ProSoft.Model.Interfaces
{
    public interface IBackupService
    {
        Task<bool> ExecuteBackupJob(string jobId);
        Task<List<bool>> ExecuteAllBackupJobs();
        void AddBackupJob(BackupJob job);
        void UpdateBackupJob(BackupJob job);
        void DeleteBackupJob(string jobId);
        List<BackupJob> GetAllJobs();
        BackupJob GetJob(string jobId);
        bool JobExists(string jobName);
        int GetJobCount();
    }
}
