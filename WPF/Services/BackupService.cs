using Core.Model;
using Core.Model.Interfaces;
using Core.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WPF.Services
{
    public class BackupService : IBackupService
    {
        // Collections pour gérer les opérations en cours
        private Dictionary<string, CancellationTokenSource> _jobCancellationTokens = new Dictionary<string, CancellationTokenSource>();
        private Dictionary<string, ManualResetEvent> _jobPauseEvents = new Dictionary<string, ManualResetEvent>();

        // Liste des jobs stockés en mémoire ou dans un fichier
        private List<BackupJob> _jobs = new List<BackupJob>();

        // Méthodes pour gérer les jobs
        public void AddBackupJob(BackupJob job)
        {
            _jobs.Add(job);
            // Ici vous pourriez sauvegarder la liste des jobs dans un fichier
        }

        public bool DeleteBackupJob(string jobId)
        {
            var job = _jobs.FirstOrDefault(j => j.Id == jobId);
            if (job != null)
            {
                _jobs.Remove(job);
                // Ici vous pourriez sauvegarder la liste des jobs dans un fichier
                return true;
            }
            return false;
        }

        public List<BackupJob> GetAllJobs()
        {
            return _jobs;
        }

        public BackupJob GetJob(string jobId)
        {
            return _jobs.FirstOrDefault(j => j.Id == jobId);
        }

        public int GetJobCount()
        {
            return _jobs.Count;
        }

        public bool JobExists(string jobName)
        {
            return _jobs.Any(j => j.Name.Equals(jobName, StringComparison.OrdinalIgnoreCase));
        }

        public void UpdateBackupJob(BackupJob job)
        {
            var existingJob = _jobs.FirstOrDefault(j => j.Id == job.Id);
            if (existingJob != null)
            {
                int index = _jobs.IndexOf(existingJob);
                _jobs[index] = job;
                // Ici vous pourriez sauvegarder la liste des jobs dans un fichier
            }
        }

        // Méthodes pour l'exécution des jobs
        public async Task<bool> ExecuteBackupJob(string jobId, IProgress<float> progress, string keyToUse)
        {
            var job = GetJob(jobId);
            if (job == null) return false;

            // Créer le token d'annulation et l'événement de pause pour ce job
            var cts = new CancellationTokenSource();
            var pauseEvent = new ManualResetEvent(true); // Initialement non pausé (signalé)

            _jobCancellationTokens[jobId] = cts;
            _jobPauseEvents[jobId] = pauseEvent;

            try
            {
                if (!Directory.Exists(job.SourceDirectory))
                {
                    return false;
                }

                if (!Directory.Exists(job.TargetDirectory))
                {
                    Directory.CreateDirectory(job.TargetDirectory);
                }

                // Obtenir les fichiers à copier selon le type de sauvegarde
                var sourceFiles = Directory.GetFiles(job.SourceDirectory, "*", SearchOption.AllDirectories);
                float totalFiles = sourceFiles.Length;
                float filesCopied = 0;

                foreach (var sourceFile in sourceFiles)
                {
                    // Vérifier si l'opération a été annulée
                    if (cts.Token.IsCancellationRequested)
                        return false;

                    // Attendre si l'opération est en pause
                    pauseEvent.WaitOne();

                    // Calculer le chemin de destination
                    string relativePath = sourceFile.Substring(job.SourceDirectory.Length).TrimStart('\\', '/');
                    string targetFile = Path.Combine(job.TargetDirectory, relativePath);

                    // Créer le répertoire cible s'il n'existe pas
                    Directory.CreateDirectory(Path.GetDirectoryName(targetFile));

                    // Copier le fichier
                    File.Copy(sourceFile, targetFile, true);

                    // Mettre à jour la progression
                    filesCopied++;
                    progress?.Report(filesCopied / totalFiles);
                }

                return true;
            }
            catch (OperationCanceledException)
            {
                return false;
            }
            catch (Exception)
            {
                return false;
            }
            finally
            {
                // Nettoyer les ressources
                if (_jobCancellationTokens.ContainsKey(jobId))
                    _jobCancellationTokens.Remove(jobId);

                if (_jobPauseEvents.ContainsKey(jobId))
                    _jobPauseEvents.Remove(jobId);
            }
        }

        public async Task<List<bool>> ExecuteAllBackupJobs(IProgress<float> progress)
        {
            var results = new List<bool>();
            float jobCount = _jobs.Count;
            float jobsDone = 0;

            foreach (var job in _jobs)
            {
                var jobProgress = new Progress<float>(p =>
                {
                    // Calculer la progression globale
                    float overallProgress = (jobsDone + p) / jobCount;
                    progress?.Report(overallProgress);
                });

                bool result = await ExecuteBackupJob(job.Id, jobProgress, null);
                results.Add(result);
                jobsDone++;
            }

            return results;
        }

        // Méthodes pour la pause, la reprise et l'arrêt
        public Task PauseBackupJob(string jobId)
        {
            if (_jobPauseEvents.TryGetValue(jobId, out var pauseEvent))
            {
                // Mettre l'événement en état non signalé pour pauser l'opération
                pauseEvent.Reset();
            }

            return Task.CompletedTask;
        }

        public Task ResumeBackupJob(string jobId)
        {
            if (_jobPauseEvents.TryGetValue(jobId, out var pauseEvent))
            {
                // Signaler l'événement pour reprendre l'opération
                pauseEvent.Set();
            }

            return Task.CompletedTask;
        }

        public Task StopBackupJob(string jobId)
        {
            if (_jobCancellationTokens.TryGetValue(jobId, out var cts))
            {
                // Annuler l'opération
                cts.Cancel();
            }

            // Nettoyer les ressources
            if (_jobPauseEvents.ContainsKey(jobId))
                _jobPauseEvents.Remove(jobId);

            if (_jobCancellationTokens.ContainsKey(jobId))
                _jobCancellationTokens.Remove(jobId);

            return Task.CompletedTask;
        }

        // Méthode pour l'encryption/décryption
        public async Task<bool> Encryption(bool isEncrypted, BackupJob job, string key, IProgress<float> progress)
        {
            try
            {
                // Créer un token d'annulation et un événement de pause pour cette opération
                var cts = new CancellationTokenSource();
                var pauseEvent = new ManualResetEvent(true);

                _jobCancellationTokens[job.Id] = cts;
                _jobPauseEvents[job.Id] = pauseEvent;

                string directory = job.TargetDirectory;
                if (!Directory.Exists(directory))
                    return false;

                string[] files;
                if (isEncrypted)
                {
                    // Fichiers à chiffrer (non .enc)
                    files = Directory.GetFiles(directory, "*", SearchOption.AllDirectories)
                                     .Where(f => !f.EndsWith(".enc") && !f.EndsWith(".exe") && !f.EndsWith(".dll"))
                                     .ToArray();
                }
                else
                {
                    // Fichiers à déchiffrer (.enc)
                    files = Directory.GetFiles(directory, "*.enc", SearchOption.AllDirectories);
                }

                int totalFiles = files.Length;
                int processedFiles = 0;

                foreach (var file in files)
                {
                    // Vérifier si l'opération a été annulée
                    if (cts.Token.IsCancellationRequested)
                        return false;

                    // Attendre si l'opération est en pause
                    pauseEvent.WaitOne();

                    if (isEncrypted)
                    {
                        // Chiffrer le fichier
                        await EncryptFile(file, key);
                    }
                    else
                    {
                        // Déchiffrer le fichier
                        await DecryptFile(file, key);
                    }

                    // Mettre à jour la progression
                    processedFiles++;
                    progress?.Report((float)processedFiles / totalFiles);
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
            finally
            {
                // Nettoyer les ressources
                if (_jobCancellationTokens.ContainsKey(job.Id))
                    _jobCancellationTokens.Remove(job.Id);

                if (_jobPauseEvents.ContainsKey(job.Id))
                    _jobPauseEvents.Remove(job.Id);
            }
        }

        private async Task EncryptFile(string filePath, string key)
        {
            // Implémentation simplifiée - à remplacer par votre logique de chiffrement
            byte[] fileBytes = File.ReadAllBytes(filePath);
            byte[] keyBytes = Encoding.UTF8.GetBytes(key);

            // Chiffrer les données (ceci est un exemple simplifié)
            byte[] encryptedBytes = fileBytes.Select((b, i) => (byte)(b ^ keyBytes[i % keyBytes.Length])).ToArray();

            // Sauvegarder le fichier chiffré
            string encryptedPath = filePath + ".enc";
            await File.WriteAllBytesAsync(encryptedPath, encryptedBytes);

            // Supprimer le fichier original
            File.Delete(filePath);
        }

        private async Task DecryptFile(string filePath, string key)
        {
            // Implémentation simplifiée - à remplacer par votre logique de déchiffrement
            byte[] fileBytes = File.ReadAllBytes(filePath);
            byte[] keyBytes = Encoding.UTF8.GetBytes(key);

            // Déchiffrer les données (ceci est un exemple simplifié)
            byte[] decryptedBytes = fileBytes.Select((b, i) => (byte)(b ^ keyBytes[i % keyBytes.Length])).ToArray();

            // Sauvegarder le fichier déchiffré
            string decryptedPath = filePath.Substring(0, filePath.Length - 4); // Enlever '.enc'
            await File.WriteAllBytesAsync(decryptedPath, decryptedBytes);

            // Supprimer le fichier chiffré
            File.Delete(filePath);
        }
        private async Task CopyFile(string sourcePath, string targetPath, IProgress<float> fileProgress = null)
        {
            const int bufferSize = 81920; // 80KB buffer
            byte[] buffer = new byte[bufferSize];

            using (FileStream sourceStream = new FileStream(sourcePath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize, true))
            using (FileStream targetStream = new FileStream(targetPath, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize, true))
            {
                long fileSize = sourceStream.Length;
                long totalBytesRead = 0;
                int bytesRead;

                while ((bytesRead = await sourceStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                {
                    await targetStream.WriteAsync(buffer, 0, bytesRead);
                    totalBytesRead += bytesRead;

                    // Rapport de progression pour ce fichier individuel
                    fileProgress?.Report((float)totalBytesRead / fileSize);
                }
            }
        }
    }
}