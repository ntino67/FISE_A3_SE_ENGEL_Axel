using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Model.Interfaces;
using Core.Utils;
using System.Threading;

namespace Core.Model.Implementations
{
    public class JobManager : IBackupService
    {
        private List<BackupJob> _jobs;
        private readonly ILogger _logger;
        private readonly IConfigurationManager _configManager;
        private readonly IUIService _uiService;
        private readonly IResourceService _resourceService;
        private readonly object _jobsLock = new object();
        private readonly IProcessChecker _processChecker;
        private readonly ProcessMonitor _processMonitor;

        public JobManager(ILogger logger, IConfigurationManager configManager, IUIService uiService, IResourceService resourceService, IProcessChecker processChecker = null, ProcessMonitor processMonitor = null)
        {
            _logger = logger;
            _configManager = configManager;
            _uiService = uiService;
            _resourceService = resourceService;
            _processChecker = processChecker ?? new DefaultProcessChecker();
            _processMonitor = processMonitor;
            _jobs = configManager.LoadJobs();
        }

        public void AddBackupJob(BackupJob job)
        {
            if (JobExists(job.Name))
                throw new InvalidOperationException($"Un job avec le nom {job.Name} existe déjà.");

            _jobs.Add(job);
            try
            {
                _configManager.SaveJobs(_jobs);
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Erreur lors de l'ajout du job: {ex.Message}");
                _jobs.Remove(job);
                throw;
            }
        }

        public void UpdateBackupJob(BackupJob job)
        {
            lock (_jobsLock)
            {
                BackupJob existingJob = _jobs.FirstOrDefault(j => j.Id == job.Id);
                if (existingJob == null)
                    throw new InvalidOperationException($"Le job avec l'ID {job.Id} n'existe pas.");
                if (job.Name != existingJob.Name && _jobs.Any(j => j.Name == job.Name && j.Id != job.Id))
                    throw new InvalidOperationException($"Un job avec le nom {job.Name} existe déjà.");

                int index = _jobs.IndexOf(existingJob);
                _jobs[index] = job;
                try
                {
                    _configManager.SaveJobs(_jobs);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning($"Erreur lors de la mise à jour du job: {ex.Message}");
                    throw;
                }
            }
        }

        public bool DeleteBackupJob(string jobId)
        {
            lock (_jobsLock)
            {
                BackupJob job = _jobs.FirstOrDefault(j => j.Id == jobId);
                if (job == null)
                    return false;
                _jobs.Remove(job);
                try
                {
                    _configManager.SaveJobs(_jobs);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning($"Erreur lors de la suppression du job: {ex.Message}");
                    return false;
                }
                return true;
            }
        }

        public List<BackupJob> GetAllJobs()
        {
            lock (_jobsLock) { return _jobs.ToList(); }
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

        public async Task<bool> ExecuteBackupJob(string jobId, IProgress<float> progress, string encryptionKey = null)
        {
            BackupJob job = _jobs.FirstOrDefault(j => j.Id == jobId);
            if (job == null)
                return false;
            return await ExecuteBackup(job, progress, encryptionKey);
        }

        public async Task<List<bool>> ExecuteAllBackupJobs(IProgress<float> progress)
        {
            List<bool> results = new List<bool>();
            foreach (var job in _jobs)
            {
                bool result = await ExecuteBackup(job, progress);
                results.Add(result);

                if (!result && job.Status == JobStatus.Canceled)
                {
                    _logger.LogWarning("Arrêt de l'exécution des jobs suivants car une application bloquante a été détectée.");
                    break;
                }
            }
            return results;
        }

        private async Task<bool> ExecuteBackup(BackupJob job, IProgress<float> progress, string encryptionKey = null)
        {
            try
            {
                // Check if any blocking application is running before starting the backup
                List<string> blockingApps = _configManager.GetBlockingApplications();
                string detectedApp = null;

                if (blockingApps.Count > 0)
                {
                    foreach (string app in blockingApps)
                    {
                        if (!string.IsNullOrWhiteSpace(app) && _processChecker.IsProcessRunning(app))
                        {
                            detectedApp = app;
                            break;
                        }
                    }
                    if (detectedApp != null)
                    {
                        string message = _resourceService.GetString("BackupBlockedByApp", detectedApp);
                        _uiService.ShowToast(message, 5000);
                        _logger.LogWarning($"Le job {job.Name} n'a pas pu démarrer : l'application bloquante {detectedApp} est en cours d'exécution.");
                        job.Status = JobStatus.Canceled;
                        _configManager.SaveJobs(_jobs);
                        return false;
                    }
                }

                // Backup can start, add the job to the process monitor
                _processMonitor?.AddActiveJob(job);

                if (!job.IsValid())
                    return false;

                DateTime startTime = DateTime.Now;
                job.Status = JobStatus.Running;
                _configManager.SaveJobs(_jobs);

                try
                {
                    _logger.LogBackupStart(job);

                    // Create the target directory if it doesn't exist
                    Directory.CreateDirectory(job.TargetDirectory);

                    bool success = false;

                    if (job.Type == BackupType.Full)
                    {
                        success = await ExecuteFullBackup(job, progress, encryptionKey);
                    }
                    else
                    {
                        success = await ExecuteDifferentialBackup(job, encryptionKey, progress);
                    }

                    // If encryption is enabled, execute encryption
                    if (success && job.IsEncrypted)
                    {
                        var encryptProgress = new Progress<float>(value =>
                        {
                            // Consider encryption as 20% of the task
                            progress?.Report(80 + value * 0.2f);
                        });

                        var extensions = _configManager.GetEncryptionFileExtensions();
                        var wildcard = _configManager.GetEncryptionWildcard();

                        _logger.LogWarning($"Job {job.Name} - Chiffrement avec extensions: {string.Join(", ", extensions)}");
                        _logger.LogWarning($"Job {job.Name} - Chiffrement avec wildcard: {wildcard}");

                        await CryptoHelper.Encrypt(job.TargetDirectory, extensions, wildcard, encryptionKey, encryptProgress);
                    }
                    // Only update status if not paused
                    if (job.Status != JobStatus.Paused)
                    {
                        job.Status = success ? JobStatus.Completed : JobStatus.Failed;
                        job.LastRunTime = DateTime.Now;
                        _configManager.SaveJobs(_jobs);
                    }

                    TimeSpan duration = DateTime.Now - startTime;
                    _logger.LogBackupEnd(job, success, duration);

                    return success;
                }
                catch (Exception ex)
                {
                    job.Status = JobStatus.Failed;
                    try
                    {
                        _configManager.SaveJobs(_jobs);
                    }
                    catch (Exception saveEx)
                    {
                        _logger.LogWarning($"Erreur lors de la tentative de sauvegarde du statut 'Failed' pour le job {job.Name}: {saveEx.Message}");
                    }

                    TimeSpan duration = DateTime.Now - startTime;
                    _logger.LogBackupEnd(job, false, duration);
                    _logger.LogWarning($"Erreur lors de l'exécution du job {job.Name}: {ex.Message}");

                    return false;
                }
            }
            finally
            {
                // Ensure the job is removed from the active jobs list if finished (not paused)
                if (job.Status != JobStatus.Paused)
                {
                    _processMonitor?.RemoveActiveJob(job);
                }
            }
        }


        private async Task<bool> ExecuteFullBackup(BackupJob job, IProgress<float> progress, string encryptionKey)
        {
            try
            {
                DirectoryInfo sourceDir = new DirectoryInfo(job.SourceDirectory);
                FileInfo[] files = sourceDir.GetFiles("*", SearchOption.AllDirectories);

                bool targetIsEncrypted = false;
                if (Directory.Exists(job.TargetDirectory))
                {
                    var targetFiles = Directory.GetFiles(job.TargetDirectory, "*.*", SearchOption.AllDirectories);
                    bool hasEncrypted = targetFiles.Any(f => f.EndsWith(".enc"));
                    bool hasPlain = targetFiles.Any(f => !f.EndsWith(".enc") && !f.EndsWith(".exe") && !f.EndsWith(".dll"));
                    targetIsEncrypted = hasEncrypted && !hasPlain;
                }

                int totalFiles = files.Length;
                int completedFiles = 0;
                object lockObj = new object();

                // Optionnel : limite à 4 fichiers traités en parallèle
                var semaphore = new SemaphoreSlim(4);

                var tasks = files.Select(async file =>
                {
                    await semaphore.WaitAsync();
                    try
                    {
                        while (job.Status == JobStatus.Paused || (BackupJob.NumberOfPriorityJobRunning > 0 && !job.isPriorityJob))
                        {
                            await Task.Delay(200);
                        }

                        if (job.Status == JobStatus.Canceled || job.Status == JobStatus.Stopped)
                            return;

                        string relativePath = file.FullName.Substring(sourceDir.FullName.Length + 1);
                        string targetPath = Path.Combine(job.TargetDirectory, relativePath);

                        Directory.CreateDirectory(Path.GetDirectoryName(targetPath));

                        DateTime startCopy = DateTime.Now;
                        await CopyFile(file.FullName, targetPath);
                        long transferTime = (long)(DateTime.Now - startCopy).TotalMilliseconds;

                        _logger.LogBackupOperation(job.Name, file.FullName, targetPath, file.Length, transferTime, "SUCCESS");

                        if (targetIsEncrypted)
                        {
                            try
                            {
                                byte[] keyBytes = Encoding.UTF8.GetBytes(encryptionKey);
                                CryptoSoft.XorEncryption.EncryptFile(targetPath, keyBytes);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogBackupOperation(job.Name, file.FullName, targetPath, file.Length, transferTime,
                                    $"ENCRYPT_ERROR: {ex.Message}");
                            }
                        }

                        if (job.Status == JobStatus.Running)
                        {
                            lock (lockObj)
                            {
                                completedFiles++;
                                progress?.Report((float)completedFiles / totalFiles * 100);
                            }
                        }
                    }
                    finally
                    {
                        semaphore.Release();
                    }
                });

                await Task.WhenAll(tasks);
                return true;
            }
            catch (Exception e)
            {
                _logger.LogWarning($"Erreur dans ExecuteFullBackup: {e}");
                return false;
            }
        }


        private async Task<bool> ExecuteDifferentialBackup(BackupJob job, string encryptionKey, IProgress<float> progress)
        {
            try
            {
                DirectoryInfo sourceDir = new DirectoryInfo(job.SourceDirectory);
                DirectoryInfo targetDir = new DirectoryInfo(job.TargetDirectory);

                if (!targetDir.Exists)
                {
                    // Si le répertoire cible n'existe pas, effectuer une sauvegarde complète
                    return await ExecuteFullBackup(job, progress, encryptionKey);
                }

                FileInfo[] files = sourceDir.GetFiles("*", SearchOption.AllDirectories);
                
                var targetFiles = Directory.GetFiles(job.TargetDirectory, "*.*", SearchOption.AllDirectories);
                bool hasEncrypted = targetFiles.Any(f => f.EndsWith(".enc"));
                bool hasPlain = targetFiles.Any(f => !f.EndsWith(".enc") && !f.EndsWith(".exe") && !f.EndsWith(".dll"));
                bool targetIsEncrypted = hasEncrypted && !hasPlain;

                foreach (FileInfo file in files)
                {
                    while (job.Status == JobStatus.Paused || (BackupJob.NumberOfPriorityJobRunning > 0 && job.isPriorityJob == false))
                    {
                        // Attendre que le job soit relancé
                        await Task.Delay(200);
                    }
                    if (job.Status == JobStatus.Canceled)
                    {
                        _logger.LogWarning($"Le job {job.Name} a été annulé pendant la sauvegarde.");
                        return false;
                    }
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
                        
                        if (targetIsEncrypted)
                        {
                            try
                            {
                                byte[] keyBytes = Encoding.UTF8.GetBytes(encryptionKey);
                                CryptoSoft.XorEncryption.EncryptFile(targetPath, keyBytes);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogBackupOperation(job.Name, file.FullName, targetPath, file.Length, transferTime, $"ENCRYPT_ERROR: {ex.Message}");
                            }
                        }
                        progress?.Report((float)files.ToList().IndexOf(file) / files.Length * 100);
                    }
                }

                return true;
            }
            catch (Exception e)
            {
                _logger.LogWarning($"Erreur dans ExecuteDifferentialBackup: {e}");
                return false;
            }
        }

        private async Task CopyFile(string sourcePath, string targetPath)
        {
            var fileInfo = new FileInfo(sourcePath);
            var fileManager = LargeFileTransferManager.Instance;

            // Vérifier si c'est un fichier volumineux
            bool isLargeFile = fileManager.IsLargeFile(fileInfo.Length);

            if (isLargeFile)
            {
                // Acquérir la permission pour le transfert de fichier volumineux
                using (await fileManager.AcquireLargeFileTransferPermissionAsync())
                {
                    // Effectuer le transfert du fichier volumineux
                    using (FileStream sourceStream = new FileStream(sourcePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, true))
                    using (FileStream targetStream = new FileStream(targetPath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, true))
                    {
                        await sourceStream.CopyToAsync(targetStream);
                    }
                }
            }
            else
            {
                // Pour les petits fichiers, procéder comme d'habitude
                using (FileStream sourceStream = new FileStream(sourcePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, true))
                using (FileStream targetStream = new FileStream(targetPath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, true))
                {
                    await sourceStream.CopyToAsync(targetStream);
                }
            }
        }



        internal async Task<bool> Encrypt(BackupJob job, string key, IProgress<float> progress)
        {
            string Directory = job.TargetDirectory;
            _logger.LogEncryptionStart(job);

            try
            {

                List<string> extensions = _configManager.GetEncryptionFileExtensions();
                string wildcard = _configManager.GetEncryptionWildcard();

                _logger.LogWarning($"Extensions configurées: {string.Join(", ", extensions)}");
                _logger.LogWarning($"Wildcard configuré: {wildcard}");

                long duration = await CryptoHelper.Encrypt(Directory, extensions, wildcard, key, progress);

                _logger.LogEncryptionEnd(job, true, duration);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Erreur lors du chiffrement du job {job.Name}: {ex.Message}");
                _logger.LogEncryptionEnd(job, false, 0);
                return false;
            }
        }

        internal async Task<bool> Decrypt(BackupJob job, string key, IProgress<float> progress)
        {
            string Directory = job.TargetDirectory;
            _logger.LogEncryptionStart(job);
            long duration = await CryptoHelper.Decrypt(Directory, key, progress);
            _logger.LogEncryptionEnd(job, true, duration);
            return true;
        }

        public async Task<bool> Encryption(bool isEncrypted, BackupJob job, string Key, IProgress<float> progress)
        {
            job.Status = JobStatus.Running;
            if (isEncrypted)
            {
                await Encrypt(job, Key ?? "EasySave" + job.Id, progress);
            }
            else
            {
                await Decrypt(job, Key ?? "EasySave" + job.Id, progress);
            }
            job.Status = JobStatus.Completed;
            return true;
        }


    }
}
