using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Core.Model;
using Core.Model.Interfaces;
using Core.Utils;

namespace Core.ViewModel
{
    public class JobViewModel : INotifyPropertyChanged
    {
        public event Action<string> JobDeleted;
        public event Action<string, string> InstructionChanged;

        private InstructionHandlerViewModel _instructionHandler;
        private readonly IBackupService _jobManager;
        private readonly IUIService _ui;
        private readonly ICommandFactory _commandFactory;
        private readonly IConfigurationManager _configManager; 
        private FileSystemWatcher _watcher;
        private BackupJob _currentJob;
        private string _searchText;
        private ObservableCollection<BackupJob> _filteredJobs;
        public ObservableCollection<RunningInstruction> RunningInstructions => _instructionHandler.RunningInstructions;


        public BackupJob CurrentJob
        {
            get => _currentJob;
            set
            {
                if (_currentJob != value)
                {
                    if (_currentJob != null)
                        _currentJob.PropertyChanged -= OnCurrentJobPropertyChanged;

                    _currentJob = value;
                    OnPropertyChanged();

                    if (_currentJob != null)
                        _currentJob.PropertyChanged += OnCurrentJobPropertyChanged;

                    OnPropertyChanged(nameof(EncryptionStatus));
                }
            }
        }

        private void OnCurrentJobPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(BackupJob.Progress))
            {
                OnPropertyChanged(nameof(CurrentJob.Progress)); // 👈 pour notifier la vue
            }
        }

        public string CurrentlyRunningJobs
        {
            get { return Application.Current.Resources["BackupStatus"] as string + " ("+ _instructionHandler.RunningInstructions
    .Count(ri => ri?.Job.Status == JobStatus.Running) + ")"; }
        }

        public JobViewModel(IBackupService jobManager, IUIService uiService, ICommandFactory commandFactory, InstructionHandlerViewModel instructionHandlerViewModel, IConfigurationManager configManager) 
        {
            _jobManager = jobManager;
            _instructionHandler = instructionHandlerViewModel;
            _ui = uiService;
            _commandFactory = commandFactory;
            _configManager = configManager;
            _instructionHandler.RunningInstructions.CollectionChanged += (s, e) =>
            {
                if (e.NewItems != null)
                {
                    foreach (RunningInstruction ri in e.NewItems)
                        ri.Job.PropertyChanged += Job_PropertyChanged;
                }

                if (e.OldItems != null)
                {
                    foreach (RunningInstruction ri in e.OldItems)
                        ri.Job.PropertyChanged -= Job_PropertyChanged;
                }

                OnPropertyChanged(nameof(CurrentlyRunningJobs));
            };
            Jobs = new ObservableCollection<BackupJob>(_jobManager.GetAllJobs());

            RunBackupCommand = _commandFactory.Create(
                async job => await ExecuteCurrentJob((BackupJob)job),
                _ => CurrentJob?.IsValid() == true
            );

            ResetJobCommand = _commandFactory.Create(
                _ => ResetCurrentJob(),
                _ => CurrentJob != null
            );

            DeleteJobCommand = commandFactory.Create<BackupJob>(
                job =>
                {
                    if (job == null) return;
                    var choice = _ui.ConfirmDeleteJobWithFiles(job.Name, job.TargetDirectory);

                    if (choice == DeleteJobChoice.Cancel)
                        return;

                    if (choice == DeleteJobChoice.DeleteJobAndFiles)
                    {
                        try
                        {
                            if (Directory.Exists(job.TargetDirectory))
                            {
                                foreach (var file in Directory.GetFiles(job.TargetDirectory, "*", SearchOption.AllDirectories))
                                {
                                    try
                                    {
                                        File.Delete(file);
                                    }
                                    catch (Exception ex)
                                    {
                                        throw new Exception($"Error : {ex.Message}");
                                    }
                                }
                                foreach (var dir in Directory.GetDirectories(job.TargetDirectory, "*", SearchOption.AllDirectories).OrderByDescending(d => d.Length))
                                {
                                    try
                                    {
                                        Directory.Delete(dir, true);
                                    }
                                    catch (Exception ex)
                                    {
                                        throw new Exception($"Error : {ex.Message}");
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            _ui.ShowToast($"Error deleting backup files: {ex.Message}", 4000);
                        }
                    }

                    DeleteJob(job.Id);
                    NavigateToHome?.Invoke();
                },
                job => job != null
            );

            RunAllBackupsCommand = _commandFactory.Create(
                async _ => await ExecuteAllJobs(),
                _ => Jobs.Any()
            );

            RunSelectedBackupsCommand = _commandFactory.Create(
                async _ => await ExecuteSelectedJobs(),
                _ => Jobs.Any(j => j.IsChecked)
            );

            CreateJobCommand = _commandFactory.Create(
                param =>
                {
                    string name = param as string;
                    if (string.IsNullOrWhiteSpace(name))
                        return;

                    if (_jobManager.JobExists(name))
                    {
                        _ui.ShowToast("❌ Ce nom de job existe déjà.", 3000);
                        return;
                    }

                    CreateNewJob(name);
                },
                param =>
                {
                    string name = param as string;
                    return !string.IsNullOrWhiteSpace(name);
                });

            ToggleEncryptionCommand = _commandFactory.Create(
                job =>
                {
                    if (string.IsNullOrWhiteSpace(EncryptionKey))
                        _ui.ShowToast("🔑 Please enter a key first", 3000);
                    else
                        ToggleEncryption((BackupJob) job, EncryptionKey);
                },
                _ => EncryptionKey != null && EncryptionStatus != "Status: Unknown"
            );

            PauseResumeCommand = _commandFactory.Create(
                job =>
                {
                    PauseResumeJob((BackupJob)job);
                },
                _ => CurrentJob != null && (CurrentJob.Status == JobStatus.Running || CurrentJob.Status == JobStatus.Paused)
            );

            StopBackupCommand = _commandFactory.Create(
                _ => StopCurrentJob(),
                _ => CurrentJob != null && (CurrentJob.Status == JobStatus.Running || CurrentJob.Status == JobStatus.Paused)
            );
        }

        private void PauseResumeJob(BackupJob job)
        {
            if (CurrentJob.Status == JobStatus.Running)
                PauseCurrentJob((BackupJob)job);
            else if (CurrentJob.Status == JobStatus.Paused)
                ResumeCurrentJob((BackupJob)job);
        }

        public event PropertyChangedEventHandler PropertyChanged; //Ne pas toucher

        public ObservableCollection<BackupJob> Jobs { get; private set; } //Ne pas toucher
        
        public Action RefreshCommands { get; set; } = () => { }; //Ne pas toucher

        public Action<Action> RunOnUiThread { get; set; } = action => action(); //Ne pas toucher

        public Action NavigateToHome { get; set; } = () => { }; //Ne pas toucher
        
        private string _encryptionKey; //Ne pas toucher
        public string EncryptionKey //Ne pas toucher
        {
            get => _encryptionKey; //Ne pas toucher
            set
            {
                if (_encryptionKey != value)
                {
                    _encryptionKey = value;
                    OnPropertyChanged();
                }
            }
        }

        private void Job_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(BackupJob.Status))
                OnPropertyChanged(nameof(CurrentlyRunningJobs));
        }
        public string SourceDirectoryLabel => "📁 "+ Application.Current.Resources["Source"] as string + ": " + TrimPath(CurrentJob?.SourceDirectory);
        public string TargetDirectoryLabel => "🎯 "+ Application.Current.Resources["Target"] as string + ": " + TrimPath(CurrentJob?.TargetDirectory);

        public async Task ExecuteAllJobs()
        {

            foreach (var job in Jobs)
            {
                var localJob = job; // Capture locale pour éviter les problèmes de closure
                var progress = new Progress<float>(p =>
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        localJob.Progress = p;
                    });
                });

                ShouldItBePriority(localJob);
                if (localJob.isPriorityJob)
                {
                    BackupJob.IncrementPriorityJobCount();
                }


                _instructionHandler.AddToQueue(localJob, Instruction.Backup);
                ExecuteCurrentJob(localJob);
            }

        }


        public async Task ExecuteSelectedJobs()
        {
            //var tasks = new List<Task<bool>>();

            foreach (var job in Jobs.Where(j => j.IsChecked))
            {
                var localJob = job; // Capture locale pour éviter les problèmes de closure
                var progress = new Progress<float>(p =>
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        job.Progress = p;
                    });
                });
                ShouldItBePriority(job); 
                if (job.isPriorityJob)
                {
                    BackupJob.IncrementPriorityJobCount(); // Utiliser la méthode thread-safe
                }

                // Ajouter la tâche à la liste sans await
                //var jobTask = Task.Run(async () => {
                _instructionHandler.AddToQueue(job, Instruction.Backup);
                ExecuteCurrentJob(job);
                //});

                //tasks.Add(jobTask);
            }

            // Attendre que toutes les tâches soient terminées
            //await Task.WhenAll(tasks);

            _ui.ShowToast("✅ Sauvegardes sélectionnées terminées.", 3000);
        }

        public string EncryptionStatus
        {
            get
            {
                if (CurrentJob == null || string.IsNullOrWhiteSpace(CurrentJob.TargetDirectory))
                    return "Status: Unknown";

                if (!Directory.Exists(CurrentJob.TargetDirectory))
                    return "Status: Folder missing";

                string[] files = Directory.GetFiles(CurrentJob.TargetDirectory, "*", SearchOption.AllDirectories);
                bool hasEncrypted = files.Any(f => f.EndsWith(".enc"));
                bool hasPlain = files.Any(f => !f.EndsWith(".enc") && !f.EndsWith(".exe") && !f.EndsWith(".dll"));

                return hasEncrypted && hasPlain ? "Decrypt"
                     : hasEncrypted ? "Decrypt"
                     : hasPlain ? "Encrypt"
                     : "Status: Empty";
            }
        }
        
        public ICommand RunBackupCommand { get; private set; }
        public ICommand ResetJobCommand { get; private set; }
        public ICommand DeleteJobCommand { get; private set; }
        public ICommand RunAllBackupsCommand { get; private set; }
        public ICommand RunSelectedBackupsCommand { get; private set; }
        public ICommand CreateJobCommand { get; private set; }
        public ICommand ToggleEncryptionCommand { get; private set; }
        public ICommand PauseResumeCommand { get; private set; }
        public ICommand StopBackupCommand { get; private set; }

        public void SetCurrentJob(BackupJob job)
        {
            CurrentJob = job;
            StartWatchingCurrentJobDirectory();
        }

        public void CreateNewJob(string name)
        {
            var job = new BackupJob { Name = name };
            _jobManager.AddBackupJob(job); // Persiste dans le JSON

            Jobs.Add(job);
            OnPropertyChanged(nameof(Jobs)); // Notifie la vue et les VM abonnés

            CurrentJob = job;

            _ui.ShowToast("✅ " + Application.Current.Resources["JobCreated"] as string + ".", 2000);
        }

        public void UpdateJobName(string newName)
        {
            if (CurrentJob == null)
                throw new InvalidOperationException(Application.Current.Resources["NoJobSelected"] as string);

            if (_jobManager.JobExists(newName) && CurrentJob.Name != newName)
                throw new InvalidOperationException(Application.Current.Resources["JobWithName"] as string + newName + Application.Current.Resources["AlreadyExist"] as string);

            CurrentJob.Name = newName;
            _jobManager.UpdateBackupJob(CurrentJob);
        }

        public void UpdateSourcePath(string sourcePath)
        {
            if (CurrentJob == null) throw new InvalidOperationException(Application.Current.Resources["NoJobSelected"] as string);
            CurrentJob.SourceDirectory = sourcePath;
            _jobManager.UpdateBackupJob(CurrentJob);
            OnPropertyChanged(nameof(SourceDirectoryLabel));
            RefreshCommands();
        }

        public void UpdateTargetPath(string targetPath)
        {
            if (CurrentJob == null) throw new InvalidOperationException(Application.Current.Resources["NoJobSelected"] as string);
            CurrentJob.TargetDirectory = targetPath;
            _jobManager.UpdateBackupJob(CurrentJob);
            OnPropertyChanged(nameof(TargetDirectoryLabel));
            OnPropertyChanged(nameof(EncryptionStatus));
            RefreshCommands();
        }

        public void UpdateBackupType(BackupType type)
        {
            if (CurrentJob == null) throw new InvalidOperationException(Application.Current.Resources["NoJobSelected"] as string);
            CurrentJob.Type = type;
            _jobManager.UpdateBackupJob(CurrentJob);
        }

        public async Task<bool> ExecuteCurrentJob(BackupJob CurrentJob)
        {
            if (CurrentJob == null) throw new InvalidOperationException(Application.Current.Resources["NoJobSelected"] as string);
            if (CurrentJob.Status == JobStatus.Running)
            {
                _ui.ShowToast("🔄 " + Application.Current.Resources["JobAlreadyRunning"] + ".", 3000);
                return false;
            }

            string keyToUse = this.EncryptionKey;
            var progress = new Progress<float>(p =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    CurrentJob.Progress = p;
                });
            });

            _instructionHandler.AddToQueue(CurrentJob, Instruction.Backup);
            ShouldItBePriority(CurrentJob);

            try
            {
                if (CurrentJob.isPriorityJob)
                {
                    BackupJob.IncrementPriorityJobCount();
                }

                bool result = await _jobManager.ExecuteBackupJob(CurrentJob.Id, progress, keyToUse);

                OnPropertyChanged(nameof(EncryptionStatus));

                if (result && CurrentJob.Status == JobStatus.Completed)
                {
                    _ui.ShowToast("✅ " + Application.Current.Resources["BackupComplete"] as string + "!", 3000);
                }

                return result;
            }
            finally
            {
                if (CurrentJob.isPriorityJob)
                {
                    BackupJob.DecrementPriorityJobCount();
                }
            }
        }


        private void ShouldItBePriority(BackupJob currentJob)
        {
            if (currentJob == null || string.IsNullOrWhiteSpace(currentJob.SourceDirectory)) return;

            // Utiliser le répertoire SOURCE pour vérifier les fichiers qui seront sauvegardés
            if (!Directory.Exists(currentJob.SourceDirectory)) return;

            // Récupérer les extensions prioritaires depuis la configuration
            var priorityExtensions = _configManager.GetPriorityFileExtensions();
            if (priorityExtensions == null || !priorityExtensions.Any())
                return; // Si pas d'extensions prioritaires configurées, ce n'est pas un job prioritaire

            string[] files = Directory.GetFiles(currentJob.SourceDirectory, "*", SearchOption.AllDirectories);

            bool hasPriorityFiles = files.Any(f => priorityExtensions.Any(ext =>
                f.EndsWith(ext, StringComparison.OrdinalIgnoreCase)));

            currentJob.isPriorityJob = hasPriorityFiles;
        }

        public void ResetCurrentJob()
        {
            if (CurrentJob == null) throw new InvalidOperationException(Application.Current.Resources["NoJobSelected"] as string);

            CurrentJob.Reset();
            _jobManager.UpdateBackupJob(CurrentJob);
            RefreshJobBindings();
            _ui.ShowToast("♻️ "+ Application.Current.Resources["JobReset"] as string + ".", 3000);
        }

        public void DeleteJob(string jobId)
        {
            var job = Jobs.FirstOrDefault(j => j.Id == jobId);
            if (job == null)
                throw new InvalidOperationException(Application.Current.Resources["JobDoesNotExist"] as string);
            job.Status = JobStatus.Stopped; // Set status to Deleted before removing
            _jobManager.DeleteBackupJob(jobId);
            Jobs.Remove(job);

            if (CurrentJob?.Id == jobId)
                CurrentJob = null;

            OnPropertyChanged(nameof(EncryptionStatus));
            _ui.ShowToast("🗑️ "+ Application.Current.Resources["JobDeleted"] as string + ".", 3000);
            
            // Notifier les abonnés (ex: BackupStatusPage) de la suppression du job
            JobDeleted?.Invoke(jobId);
        }
        public void PauseCurrentJob(BackupJob CurrentJob)
        {
            if (CurrentJob == null) throw new InvalidOperationException(Application.Current.Resources["NoJobSelected"] as string);
            if (CurrentJob.Status != JobStatus.Running)
            {
                _ui.ShowToast("🔄 " + Application.Current.Resources["JobNotRunning"] as string + ".", 3000);
                return;
            }
            if (CurrentJob.isPriorityJob) {
                BackupJob.DecrementPriorityJobCount(); // Decrement priority job count if it was a priority job
            }
            CurrentJob.Status = JobStatus.Paused;
            _jobManager.UpdateBackupJob(CurrentJob);
            _ui.ShowToast("⏸️ " + Application.Current.Resources["JobPaused"] as string + ".", 3000);
        }
        public void ResumeCurrentJob(BackupJob CurrentJob)
        {
            if (CurrentJob == null) throw new InvalidOperationException(Application.Current.Resources["NoJobSelected"] as string);
            if (CurrentJob.Status != JobStatus.Paused)
            {
                _ui.ShowToast("🔄 " + Application.Current.Resources["JobNotPaused"] as string + ".", 3000);
                return;
            }
            if (CurrentJob.isPriorityJob)
            {
                BackupJob.IncrementPriorityJobCount(); // Decrement priority job count if it was a priority job
            }
            CurrentJob.Status = JobStatus.Running;
            _jobManager.UpdateBackupJob(CurrentJob);
            _ui.ShowToast("▶️ " + Application.Current.Resources["JobResumed"] as string + ".", 3000);
        }
        public void StopCurrentJob()
        {
            if (CurrentJob == null) throw new InvalidOperationException(Application.Current.Resources["NoJobSelected"] as string);
            if (CurrentJob.Status != JobStatus.Running && CurrentJob.Status != JobStatus.Paused)
            {
                _ui.ShowToast("🔄 " + Application.Current.Resources["JobNotRunningOrPaused"] as string + ".", 3000);
                return;
            }
            CurrentJob.Status = JobStatus.Stopped;
            CurrentJob.Progress = 0; // Reset progress when stopping
            _jobManager.UpdateBackupJob(CurrentJob);
            _ui.ShowToast("🛑 " + Application.Current.Resources["JobStopped"] as string + ".", 3000);
        }
        public async void ToggleEncryption(BackupJob CurrentJob ,string key)
        {
            if (CurrentJob == null) return;
            if (CurrentJob.Status == JobStatus.Running)
            {
                _ui.ShowToast("🔄 "+ Application.Current.Resources["JobAlreadyRunning"] as string +".", 3000);
                return;
            }
            string folder = CurrentJob.TargetDirectory;
            if (!Directory.Exists(folder))
                throw new DirectoryNotFoundException(Application.Current.Resources["DirectoryDoesNotExist"] as string + ".");

            string[] files = Directory.GetFiles(folder, "*", SearchOption.AllDirectories);
            byte[] keyBytes = Encoding.UTF8.GetBytes(key);

            var encrypted = files.Where(f => f.EndsWith(".enc")).ToList();
            var plain = files.Where(f => !f.EndsWith(".enc") && !f.EndsWith(".exe") && !f.EndsWith(".dll")).ToList();

            var progress = new Progress<float>(p =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    CurrentJob.Progress = p;
                    OnPropertyChanged(nameof(CurrentJob.Progress));
                });
            });

            // Définir le statut et l'heure de début
            CurrentJob.Status = JobStatus.Running;
            CurrentJob.StartTime = DateTime.Now;
            OnPropertyChanged(nameof(CurrentJob.StartTime));
            OnPropertyChanged(nameof(CurrentJob.Status));

            if (encrypted.Any())
            {
                _instructionHandler.AddToQueue(CurrentJob, Instruction.Decrypt);
                InstructionChanged?.Invoke(CurrentJob.Id, Instruction.Decrypt.ToString()); // <-- S'assure que "Decrypt" est bien envoyé
                await _jobManager.Encryption(false, CurrentJob, key, progress);
                _ui.ShowToast("🔓" + Application.Current.Resources["FilesDecrypted"] as string, 3000);
            }
            else
            {
                _instructionHandler.AddToQueue(CurrentJob, Instruction.Encrypt);
                InstructionChanged?.Invoke(CurrentJob.Id, Instruction.Encrypt.ToString());
                await _jobManager.Encryption(true, CurrentJob, key, progress);
                _ui.ShowToast("🔒" + Application.Current.Resources["FilesEncrypted"] as string, 3000);
            }

            // Définir l'heure de fin et le statut
            CurrentJob.EndTime = DateTime.Now;
            CurrentJob.Status = JobStatus.Completed;
            OnPropertyChanged(nameof(CurrentJob.EndTime));
            OnPropertyChanged(nameof(CurrentJob.Status));
            OnPropertyChanged(nameof(CurrentJob.Progress));
            OnPropertyChanged(nameof(EncryptionStatus));
        }
        
        public bool IsActive
        {
            get => CurrentJob?.IsActive ?? false;
            set
            {
                if (CurrentJob != null && CurrentJob.IsActive != value)
                {
                    CurrentJob.IsActive = value;
                    _jobManager.UpdateBackupJob(CurrentJob);
                    OnPropertyChanged();
                }
            }
        }
        
        public string SearchText
        {
            get => _searchText;
            set
            {
                if (_searchText != value)
                {
                    _searchText = value;
                    OnPropertyChanged();
                    FilterJobs(_searchText);
                }
            }
        }

        // Propriété pour accéder aux jobs filtrés ou à tous les jobs si pas de filtre
        public ObservableCollection<BackupJob> DisplayedJobs => _filteredJobs ?? Jobs;

        // Méthode pour filtrer les jobs en fonction du texte de recherche
        public void FilterJobs(string searchText)
        {
            if (string.IsNullOrEmpty(searchText))
            {
                // Réinitialiser la vue avec tous les jobs
                _filteredJobs = null;
                OnPropertyChanged(nameof(DisplayedJobs));
            }
            else
            {
                _filteredJobs = new ObservableCollection<BackupJob>(
                    Jobs.Where(j => j.Name.ToLower().Contains(searchText.ToLower())));
                OnPropertyChanged(nameof(DisplayedJobs));
            }
        }

        // Méthode pour réinitialiser les sélections de tous les jobs
        public void ResetAllJobSelections()
        {
            foreach (var job in Jobs)
            {
                job.IsChecked = false;
            }
            // Sauvegarder les modifications
            RefreshJobsList();
        }

        // Méthode pour rafraîchir la liste des jobs depuis le gestionnaire
        public void RefreshJobsList()
        {
            var updatedJobs = _jobManager.GetAllJobs();
            Jobs.Clear();
            foreach (var job in updatedJobs)
            {
                Jobs.Add(job);
            }
            OnPropertyChanged(nameof(Jobs));
            OnPropertyChanged(nameof(DisplayedJobs));
        }

        private void RefreshJobBindings()
        {
            OnPropertyChanged(nameof(SourceDirectoryLabel));
            OnPropertyChanged(nameof(TargetDirectoryLabel));
            OnPropertyChanged(nameof(EncryptionStatus));
        }
        
        private string TrimPath(string path, int maxLength = 40)
        {
            if (string.IsNullOrWhiteSpace(path))
                return "["+ Application.Current.Resources["NotSet"] as string + "]";
            if (path.Length <= maxLength)
                return path;

            return "..." + path.Substring(path.Length - maxLength);
        }

        public void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        
        private void StartWatchingCurrentJobDirectory()
        {
            if (_watcher != null)
            {
                _watcher.EnableRaisingEvents = false;
                _watcher.Dispose();
                _watcher = null;
            }

            if (CurrentJob == null || string.IsNullOrWhiteSpace(CurrentJob.TargetDirectory) || !Directory.Exists(CurrentJob.TargetDirectory))
                return;

            _watcher = new FileSystemWatcher(CurrentJob.TargetDirectory)
            {
                IncludeSubdirectories = true,
                EnableRaisingEvents = true,
                NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite | NotifyFilters.DirectoryName
            };

            _watcher.Changed += (s, e) => OnDirectoryChanged();
            _watcher.Created += (s, e) => OnDirectoryChanged();
            _watcher.Deleted += (s, e) => OnDirectoryChanged();
            _watcher.Renamed += (s, e) => OnDirectoryChanged();
        }

        private void OnDirectoryChanged()
        {
            RunOnUiThread(() =>
            {
                OnPropertyChanged(nameof(EncryptionStatus));
                RefreshCommands?.Invoke();
            });
        }

        public void CreateNewJobAndClearSearch(string name)
        {
            // Créer le nouveau job comme avant
            CreateNewJob(name);

            // Vider explicitement SearchText
            SearchText = string.Empty;

            // Forcer l'actualisation des collections filtrées
            FilterJobs(string.Empty);

            // Notifier les changements
            OnPropertyChanged(nameof(SearchText));
            OnPropertyChanged(nameof(DisplayedJobs));
        }
    }
}