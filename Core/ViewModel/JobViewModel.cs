using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Core.Model;
using Core.Model.Interfaces;
using Core.Utils;

namespace Core.ViewModel
{
    public class JobViewModel : INotifyPropertyChanged
    {
        private readonly IBackupService _jobManager;
        private readonly IUIService _ui;
        private readonly ICommandFactory _commandFactory;
        private FileSystemWatcher _watcher;
        private BackupJob _currentJob;
        private float _progress;
        public float Progress
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

        public JobViewModel(IBackupService jobManager, IUIService uiService, ICommandFactory commandFactory)
        {
            _jobManager = jobManager;
            _ui = uiService;
            _commandFactory = commandFactory;

            Jobs = new ObservableCollection<BackupJob>(_jobManager.GetAllJobs());

            RunBackupCommand = _commandFactory.Create(
                async _ => await ExecuteCurrentJob(),
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
                _ =>
                {
                    if (string.IsNullOrWhiteSpace(EncryptionKey))
                        _ui.ShowToast("🔑 Please enter a key first", 3000);
                    else
                        ToggleEncryption(EncryptionKey);
                },
                _ => EncryptionKey != null && EncryptionStatus != "Status: Unknown"
            );
        }
        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<BackupJob> Jobs { get; private set; }
        
        public Action RefreshCommands { get; set; } = () => { };
        
        public Action<Action> RunOnUiThread { get; set; } = action => action();
        
        public Action NavigateToHome { get; set; } = () => { };

        public BackupJob CurrentJob
        {
            get => _currentJob;
            private set
            {
                _currentJob = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(EncryptionStatus));
            }
        }

        private string _jobMessage;
        public string JobMessage
        {
            get => _jobMessage;
            set
            {
                _jobMessage = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasJobMessage));
            }
        }
        
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

        public bool HasJobMessage => !string.IsNullOrWhiteSpace(JobMessage);

        public string SourceDirectoryLabel => "📁 Source: " + TrimPath(CurrentJob?.SourceDirectory);
        public string TargetDirectoryLabel => "🎯 Target: " + TrimPath(CurrentJob?.TargetDirectory);

        public async Task ExecuteAllJobs()
        {
            foreach (var job in Jobs)
            {
                await _jobManager.ExecuteBackupJob(job.Id, this.EncryptionKey);
            }
            _ui.ShowToast("✅ Toutes les sauvegardes sont terminées.", 3000);
        }

        public async Task ExecuteSelectedJobs()
        {
            foreach (var job in Jobs.Where(j => j.IsChecked))
            {
                await _jobManager.ExecuteBackupJob(job.Id, this.EncryptionKey);
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

            _ui.ShowToast("✅ Job ajouté.", 2000);
        }

        public void UpdateJobName(string newName)
        {
            if (CurrentJob == null)
                throw new InvalidOperationException("Aucun job n'est sélectionné.");

            if (_jobManager.JobExists(newName) && CurrentJob.Name != newName)
                throw new InvalidOperationException($"Un job avec le nom {newName} existe déjà.");

            CurrentJob.Name = newName;
            _jobManager.UpdateBackupJob(CurrentJob);
        }

        public void UpdateSourcePath(string sourcePath)
        {
            if (CurrentJob == null) throw new InvalidOperationException("Aucun job n'est sélectionné.");
            CurrentJob.SourceDirectory = sourcePath;
            _jobManager.UpdateBackupJob(CurrentJob);
            OnPropertyChanged(nameof(SourceDirectoryLabel));
            RefreshCommands();
        }

        public void UpdateTargetPath(string targetPath)
        {
            if (CurrentJob == null) throw new InvalidOperationException("Aucun job n'est sélectionné.");
            CurrentJob.TargetDirectory = targetPath;
            _jobManager.UpdateBackupJob(CurrentJob);
            OnPropertyChanged(nameof(TargetDirectoryLabel));
            OnPropertyChanged(nameof(EncryptionStatus));
            RefreshCommands();
        }

        public void UpdateBackupType(BackupType type)
        {
            if (CurrentJob == null) throw new InvalidOperationException("Aucun job n'est sélectionné.");
            CurrentJob.Type = type;
            _jobManager.UpdateBackupJob(CurrentJob);
        }

        public async Task<bool> ExecuteCurrentJob()
        {
            if (CurrentJob == null) throw new InvalidOperationException("Aucun job n'est sélectionné.");
            if (CurrentJob.Status == JobStatus.Running)
            {
                _ui.ShowToast("🔄 Job déjà en cours d'exécution.", 3000);
                return false;
            }
            Progress<float> progress = new Progress<float>(value => Progress = value);
            string keyToUse = this.EncryptionKey;
            bool result = await _jobManager.ExecuteBackupJob(CurrentJob.Id, progress, keyToUse);
            OnPropertyChanged(nameof(EncryptionStatus));
            _ui.ShowToast("✅ Backup complete!", 3000);
            return result;
        }

        public void ResetCurrentJob()
        {
            if (CurrentJob == null) throw new InvalidOperationException("Aucun job n'est sélectionné.");

            CurrentJob.Reset();
            _jobManager.UpdateBackupJob(CurrentJob);
            RefreshJobBindings();
            _ui.ShowToast("♻️ Job reset.", 3000);
        }

        public async void DeleteJob(string jobId)
        {
            var job = Jobs.FirstOrDefault(j => j.Id == jobId);
            if (job == null)
                throw new InvalidOperationException("Chosen job doesn't exist.");

            await _jobManager.DeleteBackupJob(jobId);
            Jobs.Remove(job);

            if (CurrentJob?.Id == jobId)
                CurrentJob = null;

            OnPropertyChanged(nameof(EncryptionStatus));
            _ui.ShowToast("🗑️ Job deleted.", 3000);
        }

        public async void ToggleEncryption(string key)
        {
            if (CurrentJob == null) return;
            if (CurrentJob.Status == JobStatus.Running)
            {
                _ui.ShowToast("🔄 Job déjà en cours d'exécution.", 3000);
                return;
            }
            string folder = CurrentJob.TargetDirectory;
            if (!Directory.Exists(folder))
                throw new DirectoryNotFoundException("Target directory not found.");

            string[] files = Directory.GetFiles(folder, "*", SearchOption.AllDirectories);
            byte[] keyBytes = Encoding.UTF8.GetBytes(key);

            var encrypted = files.Where(f => f.EndsWith(".enc")).ToList();
            var plain = files.Where(f => !f.EndsWith(".enc") && !f.EndsWith(".exe") && !f.EndsWith(".dll")).ToList();

           Progress <float> progress = new Progress<float>(value => Progress = value);

            if (encrypted.Any())
            {
                await _jobManager.Encryption(false, CurrentJob, key, progress);
                _ui.ShowToast("🔓 Files decrypted", 3000);
            }
            else
            {
                await _jobManager.Encryption(true, CurrentJob, key, progress);
                _ui.ShowToast("🔒 Files encrypted", 3000);
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
                return "[Not set]";
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