using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Collections.ObjectModel;
using Core.Model;
using Core.Model.Interfaces;
using Core.Utils;
using System.Linq;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace Core.ViewModel
{
    public class JobViewModel : INotifyPropertyChanged
    {
        private readonly IBackupService _jobManager;
        private BackupJob _currentJob;

        public string SourcePath => CurrentJob?.SourceDirectory;
        public string TargetPath => CurrentJob?.TargetDirectory;
        public event PropertyChangedEventHandler PropertyChanged;
        public ObservableCollection<BackupJob> Jobs { get; private set; }
        
        public RelayCommand RunBackupCommand { get; private set; }
        public RelayCommand ResetJobCommand { get; private set; }
        public RelayCommand CreateJobCommand { get; private set; }


        public JobViewModel(IBackupService jobManager)
        {
            _jobManager = jobManager;
            Jobs = new ObservableCollection<BackupJob>(_jobManager.GetAllJobs());
            RunBackupCommand = new RelayCommand(
                async _ => await ExecuteCurrentJob(),
                _ => CurrentJob?.IsValid() == true
            );

            ResetJobCommand = new RelayCommand(
                _ => ResetCurrentJob(),
                _ => CurrentJob != null
            );

            CreateJobCommand = new RelayCommand(
                param =>
                {
                    string name = param as string;
                    if (!string.IsNullOrWhiteSpace(name))
                        CreateNewJob(name);
                },
                param =>
                {
                    string name = param as string;
                    return !string.IsNullOrWhiteSpace(name) 
                        && !_jobManager.JobExists(name) 
                        && _jobManager.GetJobCount() < 5;
                }
            );
        }
        
        private string TrimPath(string path, int maxLength = 40)
        {
            if (string.IsNullOrWhiteSpace(path))
                return "[Not set]";
            if (path.Length <= maxLength)
                return path;
    
            // Keep only the last part
            return "..." + path.Substring(path.Length - maxLength);
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

        public bool HasJobMessage => !string.IsNullOrWhiteSpace(JobMessage);


        public string SourceDirectoryLabel =>
            "📁 Source: " + TrimPath(CurrentJob?.SourceDirectory);

        public string TargetDirectoryLabel =>
            "🎯 Target: " + TrimPath(CurrentJob?.TargetDirectory);
        
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

                return hasEncrypted && hasPlain
                    ? "Status: ⚠️ Mixed"
                    : hasEncrypted
                        ? "Decrypt"
                        : hasPlain
                            ? "Encrypt"
                            : "Status: Empty";
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public BackupJob CurrentJob
        {
            get => _currentJob;
            private set 
            {
                _currentJob = value;
                OnPropertyChanged();
            }
        }
        
        private async void AutoClearJobMessage(int msDelay = 3000)
        {
            await Task.Delay(msDelay);
            JobMessage = string.Empty;
        }

        public void SetCurrentJob(BackupJob job)
        {
            CurrentJob = job;
            RunBackupCommand?.RaiseCanExecuteChanged();
            OnPropertyChanged(nameof(EncryptionStatus));
        }

        public void CreateNewJob(string name)
        {
            if (_jobManager.GetJobCount() >= 5)
                throw new InvalidOperationException("Le nombre maximum de jobs (5) est atteint.");

            if (_jobManager.JobExists(name))
                throw new InvalidOperationException($"Un job avec le nom {name} existe d�j�.");

            var job = new BackupJob { Name = name };
            _jobManager.AddBackupJob(job);

            Jobs.Add(job);
            CurrentJob = job;
        }

        public void UpdateJobName(string newName)
        {
            if (_currentJob == null)
                throw new InvalidOperationException("Aucun job n'est sélectionné.");

            if (_jobManager.JobExists(newName) && _currentJob.Name != newName)
                throw new InvalidOperationException($"Un job avec le nom {newName} existe déjà.");

            _currentJob.Name = newName;
            _jobManager.UpdateBackupJob(_currentJob);
        }

        public void UpdateSourcePath(string sourcePath)
        {
            if (_currentJob == null)
                throw new InvalidOperationException("Aucun job n'est sélectionné.");

            _currentJob.SourceDirectory = sourcePath;
            _jobManager.UpdateBackupJob(_currentJob);
            OnPropertyChanged(nameof(SourceDirectoryLabel));
            RunBackupCommand?.RaiseCanExecuteChanged();
        }

        public void UpdateTargetPath(string targetPath)
        {
            if (_currentJob == null)
                throw new InvalidOperationException("Aucun job n'est sélectionné.");

            _currentJob.TargetDirectory = targetPath;
            _jobManager.UpdateBackupJob(_currentJob);
            OnPropertyChanged(nameof(TargetDirectoryLabel));
            OnPropertyChanged(nameof(EncryptionStatus));
            RunBackupCommand?.RaiseCanExecuteChanged();
        }

        public void UpdateBackupType(BackupType type)
        {
            if (_currentJob == null)
                throw new InvalidOperationException("Aucun job n'est sélectionné.");

            _currentJob.Type = type;
            _jobManager.UpdateBackupJob(_currentJob);
        }

        public async Task<bool> ExecuteCurrentJob()
        {
            if (_currentJob == null)
                throw new InvalidOperationException("Aucun job n'est sélectionné.");

            bool result = await _jobManager.ExecuteBackupJob(_currentJob.Id);

            OnPropertyChanged(nameof(EncryptionStatus));
            ToastBridge.ShowToast?.Invoke("✅ Backup complete!", 3000);

            return result;
        }

        public void ResetCurrentJob()
        {
            if (_currentJob == null)
                throw new InvalidOperationException("Aucun job n'est sélectionné.");

            _currentJob.Reset();
            _jobManager.UpdateBackupJob(_currentJob);
            OnPropertyChanged(nameof(EncryptionStatus));
        }

        public string GetSourcePath()
        {
            if (_currentJob == null)
                throw new InvalidOperationException("Aucun job n'est sélectionné.");
            return _currentJob.SourceDirectory;
        }

        public void DeleteJob(string jobId)
        {
            var job = Jobs.FirstOrDefault(j => j.Id == jobId);
            if (job == null)
                throw new InvalidOperationException("Le job spécifié n'existe pas.");

            _jobManager.DeleteBackupJob(jobId);
            Jobs.Remove(job);

            if (CurrentJob != null && CurrentJob.Id == jobId)
                CurrentJob = null;
        }
        
        public void ToggleEncryption(string key)
        {
            if (CurrentJob == null)
                return;

            string folder = CurrentJob.TargetDirectory;

            if (!Directory.Exists(folder))
                throw new DirectoryNotFoundException("Target directory not found.");

            string[] files = Directory.GetFiles(folder, "*", SearchOption.AllDirectories);
            byte[] keyBytes = Encoding.UTF8.GetBytes(key);

            var encrypted = files.Where(f => f.EndsWith(".enc")).ToList();
            var plain = files.Where(f => !f.EndsWith(".enc") && !f.EndsWith(".exe") && !f.EndsWith(".dll")).ToList();

            if (encrypted.Count > 0 && plain.Count > 0)
            {
                throw new InvalidOperationException("Encryption aborted: mixed encrypted and non-encrypted files detected. Please clean the target folder.");
            }

            if (encrypted.Count > 0)
            {
                foreach (var file in encrypted)
                    CryptoSoft.XorEncryption.DecryptFile(file, keyBytes);
                ToastBridge.ShowToast?.Invoke("🔓 Files decrypted", 3000);
            }
            else
            {
                foreach (var file in plain)
                    CryptoSoft.XorEncryption.EncryptFile(file, keyBytes);
                ToastBridge.ShowToast?.Invoke("🔒 Files encrypted", 3000);
            }
            
            OnPropertyChanged(nameof(EncryptionStatus));
        }
    }
}
