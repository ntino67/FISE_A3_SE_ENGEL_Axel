using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core.Model.Interfaces
{
    public interface IBackupService
    {
        Task<bool> ExecuteBackupJob(string jobId, IProgress<float> progress, string keyToUse);
        Task<List<bool>> ExecuteAllBackupJobs(IProgress<float> progress);
        void AddBackupJob(BackupJob job);
        void UpdateBackupJob(BackupJob job);
        bool DeleteBackupJob(string jobId);
        List<BackupJob> GetAllJobs();
        BackupJob GetJob(string jobId);
        bool JobExists(string jobName);
        int GetJobCount();
        Task<bool> Encryption(bool isEncrypted,BackupJob job, string Key, IProgress <float> progress);
    }
}
