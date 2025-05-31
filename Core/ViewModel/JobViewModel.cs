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
using System.Windows.Data;
using System.Collections.Generic;
using Core.Model;
using Core.Model.Interfaces;
using Core.Utils;

namespace Core.ViewModel
{
    public class BackupStatusItem : INotifyPropertyChanged
    {
        public BackupJob Job { get; }
        public string JobName => Job.Name;
        public string Instruction => Job.CurrentInstruction.ToString();
        public float Progress
        {
            get => Job.Progress * 100;
            set { Job.Progress = value / 100f; OnPropertyChanged(nameof(Progress)); }
        }
        public DateTime? StartTime => Job.StartTime;
        public DateTime? EndTime => Job.EndTime;
        public string Status => Job.Status.ToString();

        public bool CanRun => (Job.Status == JobStatus.Paused || Job.Status == JobStatus.Stopped || 
                               Job.Status == JobStatus.Completed || Job.Status == JobStatus.Ready) && 
                               Job.IsValid();
        public bool CanPause => Job.Status == JobStatus.Running;
        public bool CanStop => Job.Status == JobStatus.Running || Job.Status == JobStatus.Paused;
        // Ajoutez cette propriété
        public Action NavigateToBackupStatus { get; set; } = () => { };
        public BackupStatusItem(BackupJob job)
        {
            Job = job;
            Job.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(Job.Status) || e.PropertyName == nameof(Job.Progress))
                {
                    OnPropertyChanged(nameof(Status));
                    OnPropertyChanged(nameof(CanRun));
                    OnPropertyChanged(nameof(CanPause));
                    OnPropertyChanged(nameof(CanStop));
                    OnPropertyChanged(nameof(Progress));
                }

                // Ajouter cette condition pour détecter les changements d'instruction
                if (e.PropertyName == nameof(Job.CurrentInstruction))
                {
                    OnPropertyChanged(nameof(Instruction));
                }
            };
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged(string propertyName)
        {
            OnPropertyChanged(propertyName);
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public class JobViewModel : INotifyPropertyChanged
    {
        public event Action<string> JobDeleted;
        public event Action<string, string> InstructionChanged;

        private InstructionHandlerViewModel _instructionHandler;
        private readonly IBackupService _jobManager;
        private readonly IUIService _ui;
        private readonly ICommandFactory _commandFactory;
        private FileSystemWatcher _watcher;
        private BackupJob _currentJob;
        private string _searchText;
        private ObservableCollection<BackupJob> _filteredJobs;

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
                OnPropertyChanged(nameof(CurrentJob.Progress));
            }
        }

        public string CurrentlyRunningJobs
        {
            get { return Application.Current.Resources["BackupStatus"] as string + " ("+ _instructionHandler.RunningInstructions
    .Count(ri => ri.Job.Status == JobStatus.Running) + ")"; }
        }

        public JobViewModel(IBackupService jobManager, IUIService uiService, ICommandFactory commandFactory, InstructionHandlerViewModel instructionHandlerViewModel)
        {
            _jobManager = jobManager ?? throw new ArgumentNullException(nameof(jobManager));
            _ui = uiService ?? throw new ArgumentNullException(nameof(uiService));
            _commandFactory = commandFactory ?? throw new ArgumentNullException(nameof(commandFactory));
            _instructionHandler = instructionHandlerViewModel ?? throw new ArgumentNullException(nameof(instructionHandlerViewModel));

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

            JobStatusItems.Clear();
            foreach (var job in Jobs)
                JobStatusItems.Add(new BackupStatusItem(job));

            RunBackupCommand = _commandFactory.Create(
                async job =>
                {
                    var backupJob = (job as BackupStatusItem)?.Job ?? job as BackupJob;
                    if (backupJob != null)
                    {
                        await ExecuteCurrentJob(backupJob);
                    }
                },
                job => {
                    var backupJob = (job as BackupStatusItem)?.Job ?? job as BackupJob;
                    return backupJob?.IsValid() == true;
                }
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
                    {
                        var backupJob = (job as BackupStatusItem)?.Job ?? CurrentJob;
                        ToggleEncryption(backupJob, EncryptionKey);
                    }
                },
                _ => EncryptionKey != null && EncryptionStatus != "Status: Unknown"
            );

            PauseResumeCommand = _commandFactory.Create(
                job =>
                {
                    var backupJob = (job as BackupStatusItem)?.Job ?? CurrentJob;
                    if (backupJob.Status == JobStatus.Running)
                        PauseCurrentJob(backupJob);
                    else if (backupJob.Status == JobStatus.Paused)
                        ResumeCurrentJob(backupJob);
                },
                _ => CurrentJob != null && (CurrentJob.Status == JobStatus.Running || CurrentJob.Status == JobStatus.Paused)
            );

            StopBackupCommand = _commandFactory.Create(
                param =>
                {
                    var backupJob = (param as BackupStatusItem)?.Job ?? param as BackupJob;
                    if (backupJob == null) return;
                    StopCurrentJob(backupJob);
                },
                param =>
                {
                    var backupJob = (param as BackupStatusItem)?.Job ?? param as BackupJob;
                    return backupJob != null && (backupJob.Status == JobStatus.Running || backupJob.Status == JobStatus.Paused);
                }
            );

            RunCommand = _commandFactory.Create(
                async (param) =>
                {
                    var item = param as BackupStatusItem;
                    if (item != null)
                    {
                        await ExecuteCurrentJob(item.Job);
                    }
                },
                (param) =>
                {
                    var item = param as BackupStatusItem;
                    return item?.CanRun == true;
                });

            PauseCommand = _commandFactory.Create(
                (param) =>
                {
                    var item = param as BackupStatusItem;
                    if (item == null) return;
                    PauseCurrentJob(item.Job);
                },
                (param) =>
                {
                    var item = param as BackupStatusItem;
                    return item != null && item.CanPause;
                });

            StopCommand = _commandFactory.Create(
                (param) =>
                {
                    var item = param as BackupStatusItem;
                    if (item == null) return;
                    StopCurrentJob(item.Job);
                },
                (param) =>
                {
                    var item = param as BackupStatusItem;
                    return item != null && item.CanStop;
                });
        }
        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<BackupJob> Jobs { get; private set; }
        
        public Action RefreshCommands { get; set; } = () => { };

        public Action<Action> RunOnUiThread { get; set; } = action => action();

        public Action NavigateToHome { get; set; } = () => { };
        
        private string _encryptionKey;
        public string EncryptionKey
        {
            get => _encryptionKey;
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
                job.StartTime = DateTime.Now;
                job.EndTime = null;
                job.Progress = 0;
                job.Status = JobStatus.Running;
                _jobManager.UpdateBackupJob(job);

                if (!JobStatusItems.Any(item => item.Job.Id == job.Id))
                {
                    JobStatusItems.Add(new BackupStatusItem(job));
                }
                
                var progress = new Progress<float>(p =>
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        job.Progress = p;
                        _jobManager.UpdateBackupJob(job);
                    });
                });
                await _jobManager.ExecuteBackupJob(job.Id, progress, this.EncryptionKey);

                job.EndTime = DateTime.Now;
                job.Status = JobStatus.Completed;
                _jobManager.UpdateBackupJob(job);
            }
            _ui.ShowToast("✅ Toutes les sauvegardes sont terminées.", 3000);
        }

        public async Task ExecuteSelectedJobs()
        {
            foreach (var job in Jobs.Where(j => j.IsChecked))
            {
                job.StartTime = DateTime.Now;
                job.EndTime = null;
                job.Progress = 0;
                job.Status = JobStatus.Running;
                _jobManager.UpdateBackupJob(job);

                if (!JobStatusItems.Any(item => item.Job.Id == job.Id))
                {
                    JobStatusItems.Add(new BackupStatusItem(job));
                }
                
                var progress = new Progress<float>(p =>
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        job.Progress = p;
                        _jobManager.UpdateBackupJob(job);
                    });
                });
                await _jobManager.ExecuteBackupJob(job.Id, progress, this.EncryptionKey);

                job.EndTime = DateTime.Now;
                job.Status = JobStatus.Completed;
                _jobManager.UpdateBackupJob(job);
            }
            _ui.ShowToast("✅ Sauvegardes sélectionnées terminées.", 3000);
        }

        public async Task<bool> ExecuteCurrentJob(BackupJob CurrentJob)
        {
            if (CurrentJob == null) throw new InvalidOperationException(Application.Current.Resources["NoJobSelected"] as string);
            
            if (!CurrentJob.IsValid())
            {
                _ui.ShowToast("❌ " + (Application.Current.Resources["JobInvalid"] as string ?? "Job is invalid") + ".", 3000);
                return false;
            }
            
            if (CurrentJob.Status == JobStatus.Running)
            {
                _ui.ShowToast("🔄 " + Application.Current.Resources["JobAlreadyRunning"] + ".", 3000);
                return false;
            }
            
            CurrentJob.StartTime = DateTime.Now;
            CurrentJob.EndTime = null;
            CurrentJob.Progress = 0;
            CurrentJob.Status = JobStatus.Running;
            _jobManager.UpdateBackupJob(CurrentJob);

            var statusItem = JobStatusItems.FirstOrDefault(i => i.Job.Id == CurrentJob.Id);
            if (statusItem == null)
            {
                statusItem = new BackupStatusItem(CurrentJob);
                JobStatusItems.Add(statusItem);
            }
            
            CurrentJob.CurrentInstruction = Instruction.Backup;
            _instructionHandler.AddToQueue(CurrentJob, Instruction.Backup);
            
            // Update instruction in status
            if (statusItem != null)
            {
                statusItem.NotifyPropertyChanged(nameof(statusItem.Instruction));
            }
            
            string keyToUse = this.EncryptionKey;
            var progress = new Progress<float>(p =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    CurrentJob.Progress = p;
                    _jobManager.UpdateBackupJob(CurrentJob);
                    // Déclencher une notification spécifique pour la progression
                    OnPropertyChanged(nameof(JobStatusItems));
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
            
            CurrentJob.EndTime = DateTime.Now;
            CurrentJob.Status = result ? JobStatus.Completed : JobStatus.Failed;
            _jobManager.UpdateBackupJob(CurrentJob);
            
            OnPropertyChanged(nameof(EncryptionStatus));
            OnPropertyChanged(nameof(JobStatusItems)); // Notifier explicitement pour rafraîchir la vue
            _ui.ShowToast("✅ "+ Application.Current.Resources["BackupComplete"] as string +"!", 3000);
            
            return result;
        }

        public void RefreshJobStatusItems()
        {
            JobStatusItems.Clear();
            foreach (var job in Jobs)
            {
                JobStatusItems.Add(new BackupStatusItem(job));
            }
            OnPropertyChanged(nameof(JobStatusItems));
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
        public ICommand RunCommand { get; }
        public ICommand PauseCommand { get; }
        public ICommand StopCommand { get; }
        public ObservableCollection<BackupStatusItem> JobStatusItems { get; } = new ObservableCollection<BackupStatusItem>();

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
            OnPropertyChanged(nameof(Jobs));

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
            job.Status = JobStatus.Stopped;
            _jobManager.DeleteBackupJob(jobId);
            Jobs.Remove(job);

            var statusItem = JobStatusItems.FirstOrDefault(item => item.Job.Id == jobId);
            if (statusItem != null)
                JobStatusItems.Remove(statusItem);

            if (CurrentJob?.Id == jobId)
                CurrentJob = null;

            OnPropertyChanged(nameof(JobStatusItems));
            OnPropertyChanged(nameof(EncryptionStatus));
            _ui.ShowToast("🗑️ "+ Application.Current.Resources["JobDeleted"] as string + ".", 3000);
            
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
        public void StopCurrentJob(BackupJob job)
        {
            if (job == null) throw new InvalidOperationException(Application.Current.Resources["NoJobSelected"] as string);
            if (job.Status != JobStatus.Running && job.Status != JobStatus.Paused)
            {
                _ui.ShowToast("🔄 " + Application.Current.Resources["JobNotRunningOrPaused"] as string + ".", 3000);
                return;
            }
            job.Status = JobStatus.Stopped;
            job.Progress = 0; // Reset progress when stopping
            _jobManager.UpdateBackupJob(job);
            _ui.ShowToast("🛑 " + Application.Current.Resources["JobStopped"] as string + ".", 3000);
        }
        public async void ToggleEncryption(BackupJob CurrentJob, string key)
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

            CurrentJob.Status = JobStatus.Running;
            CurrentJob.StartTime = DateTime.Now;
            OnPropertyChanged(nameof(CurrentJob.StartTime));
            OnPropertyChanged(nameof(CurrentJob.Status));

            if (encrypted.Any())
            {
                CurrentJob.CurrentInstruction = Instruction.Decrypt;
                _instructionHandler.AddToQueue(CurrentJob, Instruction.Decrypt);
                InstructionChanged?.Invoke(CurrentJob.Id, Instruction.Decrypt.ToString());
                await _jobManager.Encryption(false, CurrentJob, key, progress);
                _ui.ShowToast("🔓" + Application.Current.Resources["FilesDecrypted"] as string, 3000);
            }
            else
            {
                CurrentJob.CurrentInstruction = Instruction.Encrypt;
                _instructionHandler.AddToQueue(CurrentJob, Instruction.Encrypt);
                InstructionChanged?.Invoke(CurrentJob.Id, Instruction.Encrypt.ToString());
                await _jobManager.Encryption(true, CurrentJob, key, progress);
                _ui.ShowToast("🔒" + Application.Current.Resources["FilesEncrypted"] as string, 3000);
            }

            var statusItem = JobStatusItems.FirstOrDefault(i => i.Job.Id == CurrentJob.Id);
            if (statusItem != null)
            {
                statusItem.NotifyPropertyChanged(nameof(statusItem.Instruction));
            }

            CurrentJob.EndTime = DateTime.Now;
            CurrentJob.Status = JobStatus.Completed;
            CurrentJob.CurrentInstruction = Instruction.Backup;
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

        public ObservableCollection<BackupJob> DisplayedJobs => _filteredJobs ?? Jobs;

        public void FilterJobs(string searchText)
        {
            if (string.IsNullOrEmpty(searchText))
            {
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

        public void ResetAllJobSelections()
        {
            foreach (var job in Jobs)
            {
                job.IsChecked = false;
            }
            RefreshJobsList();
        }

        public void RefreshJobsList()
        {
            var updatedJobs = _jobManager.GetAllJobs();
            Jobs.Clear();
            JobStatusItems.Clear();
            foreach (var job in updatedJobs)
            {
                Jobs.Add(job);
                JobStatusItems.Add(new BackupStatusItem(job));
            }
            OnPropertyChanged(nameof(Jobs));
            OnPropertyChanged(nameof(JobStatusItems));
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
            CreateNewJob(name);
            SearchText = string.Empty;
            FilterJobs(string.Empty);
            OnPropertyChanged(nameof(SearchText));
            OnPropertyChanged(nameof(DisplayedJobs));
        }

        private void ShouldItBePriority(BackupJob currentJob)
        {
            if (currentJob == null) return;
            string[] files = Directory.GetFiles(currentJob.TargetDirectory, "*", SearchOption.AllDirectories);
            string[] priorityExtensions = { ".png" };
            bool hasPriorityFiles = files.Any(f => priorityExtensions.Any(ext => f.EndsWith(ext, StringComparison.OrdinalIgnoreCase)));
            if (hasPriorityFiles)
            {
                currentJob.isPriorityJob = true;
                return;
            }
            currentJob.isPriorityJob = false;
            return;
        }
    }
}