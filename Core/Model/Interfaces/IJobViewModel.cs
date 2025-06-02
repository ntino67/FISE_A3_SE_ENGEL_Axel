using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Core.Model;
using Core.Utils;

namespace Core.Model.Interfaces
{
    public interface IJobViewModel : INotifyPropertyChanged
    {
        ObservableCollection<BackupJob> Jobs { get; }
        ObservableCollection<BackupJob> DisplayedJobs { get; }
        BackupJob CurrentJob { get; set; }

        ICommand RunBackupCommand { get; }
        ICommand ResetJobCommand { get; }
        ICommand DeleteJobCommand { get; }
        ICommand CreateJobCommand { get; }
        ICommand ToggleEncryptionCommand { get; }
        
        string EncryptionStatus { get; }
        string EncryptionKey { get; set; }
        
        Action NavigateToHome { get; set; }

        void SetCurrentJob(BackupJob job);
        void CreateNewJob(string name);
        void UpdateJobName(string newName);
        void UpdateSourcePath(string sourcePath);
        void UpdateTargetPath(string targetPath);
        void UpdateBackupType(Core.Utils.BackupType type);
        Task<bool> ExecuteCurrentJob(BackupJob job);
        void ResetCurrentJob();
        void DeleteJob(string jobId);
        void CreateNewJobAndClearSearch(string name);
        void FilterJobs(string searchText);
        void ResetAllJobSelections();
    }
}