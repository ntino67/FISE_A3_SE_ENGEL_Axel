using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
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
        private InstructionHandlerViewModel _instructionHandler;
        private readonly IBackupService _jobManager;
        private readonly IUIService _ui;
        private readonly ICommandFactory _commandFactory;
        private FileSystemWatcher _watcher;
        private BackupJob _currentJob;
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

        private float _progress; //Devra être supprimé

        public float Progress //Devra être supprimé
        {
            get => _progress;
            set
            {
                if (_progress != value)
                {
                    _progress = value;
                    OnPropertyChanged();
                }
            }
        }

        
        public string CurrentlyRunningJobs
        {
            get { return "Backup status ("+ _instructionHandler.RunningInstructions
    .Count(ri => ri.Job.Status == JobStatus.Running) + ")"; }
        }

        public JobViewModel(IBackupService jobManager, IUIService uiService, ICommandFactory commandFactory, InstructionHandlerViewModel instructionHandlerViewModel) //Constructeur refaire toutes les commandes
        {
            _jobManager = jobManager;
            _instructionHandler = instructionHandlerViewModel;
            _ui = uiService;
            _commandFactory = commandFactory;
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
                    if (CurrentJob.Status == JobStatus.Running)
                        PauseCurrentJob((BackupJob)job);
                    else if (CurrentJob.Status == JobStatus.Paused)
                        ResumeCurrentJob((BackupJob)job);
                },
                _ => CurrentJob != null && (CurrentJob.Status == JobStatus.Running || CurrentJob.Status == JobStatus.Paused)
            );

            StopBackupCommand = _commandFactory.Create(
                _ => StopCurrentJob(),
                _ => CurrentJob != null && (CurrentJob.Status == JobStatus.Running || CurrentJob.Status == JobStatus.Paused)
            );
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
                var progress = new Progress<float>(p =>
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        job.Progress = p;
                    });
                });
                await _jobManager.ExecuteBackupJob(job.Id, progress, this.EncryptionKey);
            }
            _ui.ShowToast("✅ Toutes les sauvegardes sont terminées.", 3000);
        }

        public async Task ExecuteSelectedJobs()
        {

            foreach (var job in Jobs.Where(j => j.IsChecked))
            {
                var progress = new Progress<float>(p =>
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        job.Progress = p;
                    });
                });
                await _jobManager.ExecuteBackupJob(job.Id, progress, this.EncryptionKey);
            }
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
            _jobManager.AddBackupJob(job);

            Jobs.Add(job);
            CurrentJob = job;

            _ui.ShowToast("✅ "+ Application.Current.Resources["JobCreated"] as string + ".", 2000);
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
            if (CurrentJob.isPriorityJob)
            {
                BackupJob.NumberOfPriorityJobRunning++;
            }
            bool result = await _jobManager.ExecuteBackupJob(CurrentJob.Id, progress, keyToUse);
            if(CurrentJob.isPriorityJob)
            {
                BackupJob.NumberOfPriorityJobRunning--;
            }
            OnPropertyChanged(nameof(EncryptionStatus));


            if (result && CurrentJob.Status == JobStatus.Completed)
            {
                _ui.ShowToast("✅ " + Application.Current.Resources["BackupComplete"] as string + "!", 3000);
            }

            return result;
        }

        private void ShouldItBePriority(BackupJob currentJob)
        {
            //If the job contains .txt or .png files it pause all jobs that doesn't have job.isPriorityJob set to true
            if (currentJob == null) return;
            string[] files = Directory.GetFiles(currentJob.TargetDirectory, "*", SearchOption.AllDirectories);
            //A list of extensions that are considered priority
            string[] priorityExtensions = { ".png" };
            bool hasPriorityFiles = files.Any(f => priorityExtensions.Any(ext => f.EndsWith(ext, StringComparison.OrdinalIgnoreCase)));
            if (hasPriorityFiles)
            {
                currentJob.isPriorityJob = true; // Set the job as a priority job
                return; // This job is a priority job
            }
            currentJob.isPriorityJob = false; // Set the job as a non-priority job (usefull if something has changed since last backup)
            return; // This job is not a priority job

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
        }
        public void PauseCurrentJob(BackupJob CurrentJob)
        {
            if (CurrentJob == null) throw new InvalidOperationException(Application.Current.Resources["NoJobSelected"] as string);
            if (CurrentJob.Status != JobStatus.Running)
            {
                _ui.ShowToast("🔄 " + Application.Current.Resources["JobNotRunning"] as string + ".", 3000);
                return;
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
                });
            });
            if (encrypted.Any())
            {
                _instructionHandler.AddToQueue(CurrentJob, Instruction.Decrypt);
                await _jobManager.Encryption(false, CurrentJob, key, progress);
                _ui.ShowToast("🔓" + Application.Current.Resources["FilesDecrypted"] as string, 3000);
            }
            else
            {
                _instructionHandler.AddToQueue(CurrentJob, Instruction.Encrypt);
                await _jobManager.Encryption(true, CurrentJob, key, progress);
                _ui.ShowToast("🔒" + Application.Current.Resources["FilesEncrypted"] as string, 3000);
            }

            OnPropertyChanged(nameof(EncryptionStatus));
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

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        
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
    }
}