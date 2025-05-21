using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Core.Model.Interfaces;
using Core.Utils;

namespace Core.Model.Implementations
{
    public class JobManager : IBackupService
    {
        private const int MaxJobs = 5;
        private List<BackupJob> _jobs;
        private readonly ILogger _logger;
        private readonly IConfigurationManager _configManager;

        public JobManager(ILogger logger, IConfigurationManager configManager)
        {
            _logger = logger;
            _configManager = configManager;
            _jobs = configManager.LoadJobs();
        }

        public void AddBackupJob(BackupJob job)
        {
            if (_jobs.Count >= MaxJobs)
                throw new InvalidOperationException("Le nombre maximum de jobs (5) est atteint.");

            if (JobExists(job.Name))
                throw new InvalidOperationException($"Un job avec le nom {job.Name} existe déjà.");

            _jobs.Add(job);
            _configManager.SaveJobs(_jobs);
        }

        public void UpdateBackupJob(BackupJob job)
        {
            BackupJob existingJob = _jobs.FirstOrDefault(j => j.Id == job.Id);
            if (existingJob == null)
                throw new InvalidOperationException($"Le job avec l'ID {job.Id} n'existe pas.");

            // Vérifier si le nouveau nom n'est pas déjà utilisé par un autre job
            if (job.Name != existingJob.Name && _jobs.Any(j => j.Name == job.Name && j.Id != job.Id))
                throw new InvalidOperationException($"Un job avec le nom {job.Name} existe déjà.");

            int index = _jobs.IndexOf(existingJob);
            _jobs[index] = job;
            _configManager.SaveJobs(_jobs);
        }

        public void DeleteBackupJob(string jobId)
        {
            BackupJob job = _jobs.FirstOrDefault(j => j.Id == jobId);
            if (job == null)
                return;

            _jobs.Remove(job);
            _configManager.SaveJobs(_jobs);
        }

        public List<BackupJob> GetAllJobs()
        {
            return _jobs;
        }

        public BackupJob GetJob(string jobId)
        {
            return _jobs.FirstOrDefault(j => j.Id == jobId);
        }

        public bool JobExists(string jobName)
        {
            return _jobs.Any(j => j.Name == jobName);
        }

        public int GetJobCount()
        {
            return _jobs.Count;
        }

        public async Task<bool> ExecuteBackupJob(string jobId)
        {
            BackupJob job = _jobs.FirstOrDefault(j => j.Id == jobId);
            if (job == null)
                return false;

            return await ExecuteBackup(job);
        }

        public async Task<List<bool>> ExecuteAllBackupJobs()
        {
            List<bool> results = new List<bool>();
            foreach (var job in _jobs)
            {
                bool result = await ExecuteBackup(job);
                results.Add(result);
            }
            return results;
        }

        private async Task<bool> ExecuteBackup(BackupJob job)
        {
            if (!job.IsValid())
                return false;

            DateTime startTime = DateTime.Now;
            job.Status = JobStatus.Running;
            _configManager.SaveJobs(_jobs);

            try
            {
                _logger.LogBackupStart(job);

                // Créer le répertoire cible s'il n'existe pas
                Directory.CreateDirectory(job.TargetDirectory);

                bool success = false;

                if (job.Type == BackupType.Full)
                {
                    success = await ExecuteFullBackup(job);
                }
                else
                {
                    success = await ExecuteDifferentialBackup(job);
                }

                job.Status = success ? JobStatus.Completed : JobStatus.Failed;
                job.LastRunTime = DateTime.Now;
                _configManager.SaveJobs(_jobs);

                TimeSpan duration = DateTime.Now - startTime;
                _logger.LogBackupEnd(job, success, duration);

                return success;
            }
            catch (Exception ex)
            {
                job.Status = JobStatus.Failed;
                _configManager.SaveJobs(_jobs);

                TimeSpan duration = DateTime.Now - startTime;
                _logger.LogBackupEnd(job, false, duration);

                return false;
                throw ex;
            }
        }


        private async Task<bool> ExecuteFullBackup(BackupJob job)
        {
            try
            {
                DirectoryInfo sourceDir = new DirectoryInfo(job.SourceDirectory);
                FileInfo[] files = sourceDir.GetFiles("*", SearchOption.AllDirectories);

                foreach (FileInfo file in files)
                {
                    string relativePath = file.FullName.Substring(sourceDir.FullName.Length + 1);
                    string targetPath = Path.Combine(job.TargetDirectory, relativePath);

                    // S'assurer que le répertoire cible existe
                    Directory.CreateDirectory(Path.GetDirectoryName(targetPath));

                    DateTime startCopy = DateTime.Now;
                    await CopyFile(file.FullName, targetPath);
                    long transferTime = (long)(DateTime.Now - startCopy).TotalMilliseconds;

                    _logger.LogBackupOperation(job.Name, file.FullName, targetPath, file.Length, transferTime, "SUCCESS");
                }

                return true;
            }
            catch (Exception e)
            {
                return false;
                throw new Exception($"Error : {e}");
            }
        }

        private async Task<bool> ExecuteDifferentialBackup(BackupJob job)
        {
            try
            {
                DirectoryInfo sourceDir = new DirectoryInfo(job.SourceDirectory);
                DirectoryInfo targetDir = new DirectoryInfo(job.TargetDirectory);

                if (!targetDir.Exists)
                {
                    // Si le répertoire cible n'existe pas, effectuer une sauvegarde complète
                    return await ExecuteFullBackup(job);
                }

                FileInfo[] files = sourceDir.GetFiles("*", SearchOption.AllDirectories);

                foreach (FileInfo file in files)
                {
                    string relativePath = file.FullName.Substring(sourceDir.FullName.Length + 1);
                    string targetPath = Path.Combine(job.TargetDirectory, relativePath);

                    bool shouldCopy = true;

                    if (File.Exists(targetPath))
                    {
                        FileInfo targetFile = new FileInfo(targetPath);
                        shouldCopy = file.LastWriteTime > targetFile.LastWriteTime || file.Length != targetFile.Length;
                    }

                    if (shouldCopy)
                    {
                        // S'assurer que le répertoire cible existe
                        Directory.CreateDirectory(Path.GetDirectoryName(targetPath));

                        DateTime startCopy = DateTime.Now;
                        await CopyFile(file.FullName, targetPath);
                        long transferTime = (long)(DateTime.Now - startCopy).TotalMilliseconds;

                        _logger.LogBackupOperation(job.Name, file.FullName, targetPath, file.Length, transferTime, "SUCCESS");
                    }
                }

                return true;
            }
            catch (Exception e)
            {
                return false;
                throw new Exception($"Error : {e}");
            }
        }

        private async Task CopyFile(string sourcePath, string targetPath)
        {
            using (FileStream sourceStream = new FileStream(sourcePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, true))
            using (FileStream targetStream = new FileStream(targetPath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, true))
            {
                await sourceStream.CopyToAsync(targetStream);
            }
        }

        internal static void Encrypt(string Directory, string key)
        {
            CryptoHelper.Encrypt(Directory, new[] { ".txt", ".docx", ".xlsx", ".png" }, key);
        }

        internal static void Decrypt(string Directory, string key)
        {
            CryptoHelper.Decrypt(Directory, key);
        }

        public void Encryption(bool isEncrypted, string Directory, string Key)
        {
            if (isEncrypted)
            {
                Encrypt(Directory, Key);
            }
            else
            {
                Decrypt(Directory, Key);
            }
        }
    }
}
