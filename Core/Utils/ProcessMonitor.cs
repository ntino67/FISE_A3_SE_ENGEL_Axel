using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Core.Model;
using Core.Model.Interfaces;

namespace Core.Utils
{
    public class ProcessMonitor
    {
        private readonly IConfigurationManager _configManager;
        private readonly IUIService _uiService;
        private readonly IResourceService _resourceService;
        private readonly ILogger _logger;
        private CancellationTokenSource _cancellationTokenSource;
        private List<BackupJob> _activeJobs = new List<BackupJob>();
        private bool _transfersPaused = false;
        private readonly object _activeJobsLock = new object();

        public ProcessMonitor(IConfigurationManager configManager, IUIService uiService,
                             IResourceService resourceService, ILogger logger)
        {
            _configManager = configManager;
            _uiService = uiService;
            _resourceService = resourceService;
            _logger = logger;
        }

        public void AddActiveJob(BackupJob job)
        {
            lock (_activeJobsLock)
            {
                if (!_activeJobs.Contains(job))
                    _activeJobs.Add(job);
            }
            // Vérifier immédiatement s'il faut mettre ce job en pause
            CheckForBlockingApps();
        }

        private void CheckForBlockingApps()
        {
            List<string> blockingApps = _configManager.GetBlockingApplications();
            bool blockingAppDetected = false;
            string detectedApp = null;

            if (blockingApps.Count > 0)
            {
                foreach (string app in blockingApps)
                {
                    if (!string.IsNullOrWhiteSpace(app))
                    {
                        Process[] processes = Process.GetProcessesByName(app);
                        if (processes.Length > 0)
                        {
                            blockingAppDetected = true;
                            detectedApp = app;
                            break;
                        }
                    }
                }
            }

            lock (_activeJobsLock)
            {
                if (blockingAppDetected && !_transfersPaused)
                {
                    var runningJobs = _activeJobs.Where(j => j.Status == JobStatus.Running).ToList();
                    if (runningJobs.Any())
                    {
                        PauseAllRunningJobs(detectedApp);
                        _transfersPaused = true;
                    }
                }
                else if (!blockingAppDetected && _transfersPaused)
                {
                    var pausedJobs = _activeJobs.Where(j => j.Status == JobStatus.Paused).ToList();
                    if (pausedJobs.Any())
                    {
                        ResumeAllPausedJobs();
                        _transfersPaused = false;
                    }
                }
            }
        }

        public void RemoveActiveJob(BackupJob job)
        {
            lock (_activeJobsLock)
            {
                _activeJobs.Remove(job);
            }
        }

        public void StartMonitoring()
        {
            if (_cancellationTokenSource != null)
                StopMonitoring();

            _cancellationTokenSource = new CancellationTokenSource();
            Task.Run(() => MonitorProcesses(_cancellationTokenSource.Token));
        }

        public void StopMonitoring()
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource = null;
        }

        private async Task MonitorProcesses(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    List<string> blockingApps = _configManager.GetBlockingApplications();
                    bool blockingAppDetected = false;
                    string detectedApp = null;

                    if (blockingApps.Count > 0)
                    {
                        foreach (string app in blockingApps)
                        {
                            if (!string.IsNullOrWhiteSpace(app))
                            {
                                Process[] processes = Process.GetProcessesByName(app);
                                if (processes.Length > 0)
                                {
                                    blockingAppDetected = true;
                                    detectedApp = app;
                                    break;
                                }
                            }
                        }
                    }

                    lock (_activeJobsLock)
                    {
                        if (blockingAppDetected && !_transfersPaused)
                        {
                            var runningJobs = _activeJobs.Where(j => j.Status == JobStatus.Running).ToList();
                            if (runningJobs.Any())
                            {
                                // Pause tous les jobs en cours d'exécution
                                PauseAllRunningJobs(detectedApp);
                                _transfersPaused = true;


                            }
                        }
                        else if (!blockingAppDetected && _transfersPaused)
                        {
                            var pausedJobs = _activeJobs.Where(j => j.Status == JobStatus.Paused).ToList();
                            if (pausedJobs.Any())
                            {
                                // Reprendre tous les jobs en pause
                                ResumeAllPausedJobs();
                                _transfersPaused = false;

                                string message = _resourceService.GetString("BackupResumedAutomatically");
                                _uiService.ShowToast(message, 5000);
                                _logger.LogWarning("Jobs repris automatiquement : aucune application bloquante n'est en cours d'exécution");
                            }
                        }
                    }

                    // Vérifier toutes les 2 secondes
                    await Task.Delay(2000, token);
                }
                catch (TaskCanceledException)
                {
                    // La tâche a été annulée, quitter la boucle
                    break;
                }
                catch (Exception ex)
                {
                    // Journaliser l'erreur mais continuer la surveillance
                    _logger.LogWarning($"Erreur dans la surveillance des processus : {ex.Message}");
                    await Task.Delay(5000, token); // Délai plus long après une erreur
                }
            }
        }

        private void PauseAllRunningJobs(string detectedApp = null)
        {
            foreach (BackupJob job in _activeJobs.Where(j => j.Status == JobStatus.Running).ToList())
            {
                // Stocker l'état précédent si le job est prioritaire pour le restaurer correctement
                if (job.isPriorityJob)
                {
                    BackupJob.DecrementPriorityJobCount();
                }
                job.Status = JobStatus.Paused;
                _configManager.SaveJobs(_configManager.LoadJobs().Select(j => j.Id == job.Id ? job : j).ToList());
            }

            // Afficher la notification si detectedApp est spécifié
            if (!string.IsNullOrEmpty(detectedApp))
            {
                string message = _resourceService.GetString("BackupPausedByApp", detectedApp);
                _uiService.ShowToast(message, 5000);
                _logger.LogWarning($"Jobs mis en pause : application bloquante {detectedApp} détectée");
            }
        }

        private void ResumeAllPausedJobs()
        {
            foreach (BackupJob job in _activeJobs.Where(j => j.Status == JobStatus.Paused).ToList())
            {
                if (job.isPriorityJob)
                {
                    BackupJob.IncrementPriorityJobCount();
                }
                job.Status = JobStatus.Running;

                _configManager.SaveJobs(_configManager.LoadJobs().Select(j => j.Id == job.Id ? job : j).ToList());

            }
        }
    }
}
