using System.Collections.Generic;
using System.Threading.Tasks;
using Core.Model;

namespace Core.Model.Interfaces
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
        void Encryption(bool isEncrypted,string Directory, string Key);
    }
}
