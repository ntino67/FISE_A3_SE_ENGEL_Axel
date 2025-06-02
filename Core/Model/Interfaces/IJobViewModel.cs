using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;

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
        void UpdateSourcePath(string sourcePath);
        void UpdateTargetPath(string targetPath);
        void FilterJobs(string searchText);
        void ResetAllJobSelections();

        Task ExecuteAllJobs();
    }
}