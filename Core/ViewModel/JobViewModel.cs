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

        public string SourceDirectoryLabel =>
            "📁 Source: " + TrimPath(CurrentJob?.SourceDirectory);

        public string TargetDirectoryLabel =>
            "🎯 Target: " + TrimPath(CurrentJob?.TargetDirectory);

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

        public void SetCurrentJob(BackupJob job)
        {
            CurrentJob = job;
            RunBackupCommand?.RaiseCanExecuteChanged();
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

            return await _jobManager.ExecuteBackupJob(_currentJob.Id);
        }

        public void ResetCurrentJob()
        {
            if (_currentJob == null)
                throw new InvalidOperationException("Aucun job n'est sélectionné.");

            _currentJob.Reset();
            _jobManager.UpdateBackupJob(_currentJob);
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

            foreach (string file in files)
            {
                if (file.EndsWith(".enc"))
                {
                    // Decrypt
                    CryptoSoft.XorEncryption.DecryptFile(file, keyBytes);
                }
                else
                {
                    // Skip .exe or .enc files during encryption
                    if (file.EndsWith(".exe") || file.EndsWith(".dll") || file.EndsWith(".enc"))
                        continue;

                    // Encrypt
                    CryptoSoft.XorEncryption.EncryptFile(file, keyBytes);
                }
            }
        }
    }
}
