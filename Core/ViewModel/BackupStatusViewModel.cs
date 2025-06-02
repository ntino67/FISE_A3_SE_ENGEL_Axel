// WARN : Deprecated

using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Core.Model;
using Core.Model.Interfaces;
using Core.Utils;

namespace Core.ViewModel
{
    public class BackupStatusViewModel : ObservableObject, IBackupStatusViewModel
    {
        private readonly JobViewModel _jobViewModel;

        public ObservableCollection<BackupStatusItemModel> JobStatusItems { get; }

        public ICommand RunCommand { get; }
        public ICommand PauseCommand { get; }
        public ICommand StopCommand { get; }
        public ICommand RunAllCommand { get; }

        public BackupStatusViewModel(JobViewModel jobViewModel)
        {
            _jobViewModel = jobViewModel;
            JobStatusItems = new ObservableCollection<BackupStatusItemModel>(
                _jobViewModel.Jobs.Select(job => CreateStatusItem(job))
            );

            foreach (var job in _jobViewModel.Jobs)
            {
                job.PropertyChanged += (s, e) => SyncStatusItemWithJob(job);
                var item = JobStatusItems.FirstOrDefault(i => i.JobId == job.Id);
                if (item != null)
                {
                    SubscribeToJob(job, item);
                }
            }

            _jobViewModel.Jobs.CollectionChanged += Jobs_CollectionChanged;
            _jobViewModel.JobDeleted += OnJobDeletedFromViewModel;
            _jobViewModel.InstructionChanged += OnInstructionChangedFromViewModel;

            RunCommand = new RelayCommand<BackupStatusItemModel>(OnRunJob, CanRunJob);
            PauseCommand = new RelayCommand<BackupStatusItemModel>(OnPauseJob, CanPauseJob);
            StopCommand = new RelayCommand<BackupStatusItemModel>(OnStopJob, CanStopJob);
            RunAllCommand = new RelayCommand<object>(OnRunAllJobs, _ => JobStatusItems.Any());
        }

        private void Jobs_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (BackupJob job in e.NewItems)
                {
                    var item = CreateStatusItem(job);
                    JobStatusItems.Add(item);
                    job.PropertyChanged += (s, ev) => SyncStatusItemWithJob(job);
                    SubscribeToJob(job, item);
                }
            }
            if (e.OldItems != null)
            {
                foreach (BackupJob job in e.OldItems)
                {
                    var item = JobStatusItems.FirstOrDefault(i => i.JobId == job.Id);
                    if (item != null)
                        JobStatusItems.Remove(item);
                }
            }
        }

        private BackupStatusItemModel CreateStatusItem(BackupJob job)
        {
            return new BackupStatusItemModel
            {
                JobId = job.Id,
                JobName = job.Name,
                Instruction = Instruction.Backup.ToString(),
                Progress = job.Progress * 100,
                Status = string.IsNullOrWhiteSpace(job.SourceDirectory) || string.IsNullOrWhiteSpace(job.TargetDirectory)
                    ? "NotReady"
                    : job.Status.ToString(),
                StartTime = job.StartTime,
                EndTime = job.EndTime,
                JobReference = job,
                BytesCopied = job.BytesCopied,
                TotalBytes = job.TotalBytes
            };
        }

        private void SyncStatusItemWithJob(BackupJob job)
        {
            var item = JobStatusItems.FirstOrDefault(i => i.JobId == job.Id);
            if (item != null)
            {
                item.JobName = job.Name;
                item.Status = string.IsNullOrWhiteSpace(job.SourceDirectory) || string.IsNullOrWhiteSpace(job.TargetDirectory)
                    ? "NotReady"
                    : job.Status.ToString();
                item.StartTime = job.StartTime;
                item.EndTime = job.EndTime;
                item.BytesCopied = job.BytesCopied;
                item.TotalBytes = job.TotalBytes;
                // Progress is auto-updated by BytesCopied/TotalBytes setters
            }
        }

        private void SubscribeToJob(BackupJob job, BackupStatusItemModel item)
        {
            job.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(job.BytesCopied))
                    item.BytesCopied = job.BytesCopied;
                if (e.PropertyName == nameof(job.TotalBytes))
                    item.TotalBytes = job.TotalBytes;
                if (e.PropertyName == nameof(job.Progress))
                    item.Progress = job.Progress * 100;
                if (e.PropertyName == nameof(job.StartTime))
                    item.StartTime = job.StartTime;
                if (e.PropertyName == nameof(job.EndTime))
                    item.EndTime = job.EndTime;
            };

            job.JobStarted += (dt) => item.StartTime = dt;
            job.JobEnded += (dt) => item.EndTime = dt;
            job.StatusChanged += (status) => item.Status = status;
            job.ProgressChanged += (copied, total) =>
            {
                item.BytesCopied = copied;
                item.TotalBytes = total;
                // Progress is auto-updated by BytesCopied/TotalBytes setters
            };
        }

        private void OnJobDeletedFromViewModel(string jobId)
        {
            var item = JobStatusItems.FirstOrDefault(j => j.JobId == jobId);
            if (item != null)
                JobStatusItems.Remove(item);
        }

        private void OnInstructionChangedFromViewModel(string jobId, string instruction)
        {
            var item = JobStatusItems.FirstOrDefault(j => j.JobId == jobId);
            if (item != null)
            {
                item.Instruction = instruction;
                var job = item.JobReference;
                item.Status = string.IsNullOrWhiteSpace(job.SourceDirectory) || string.IsNullOrWhiteSpace(job.TargetDirectory)
                    ? "NotReady"
                    : job.Status.ToString();
                SyncStatusItemWithJob(job);
            }
        }

        private bool CanRunJob(BackupStatusItemModel item)
        {
            if (item?.JobReference == null)
                return false;
            var job = item.JobReference;
            if (string.IsNullOrWhiteSpace(job.SourceDirectory) || string.IsNullOrWhiteSpace(job.TargetDirectory))
                return false;
            return job.Status == JobStatus.Ready ||
                   job.Status == JobStatus.Paused ||
                   job.Status == JobStatus.Completed ||
                   job.Status == JobStatus.Failed ||
                   job.Status == JobStatus.Stopped;
        }

        private bool CanPauseJob(BackupStatusItemModel item)
        {
            return item?.JobReference != null && item.JobReference.Status == JobStatus.Running;
        }

        private bool CanStopJob(BackupStatusItemModel item)
        {
            return item?.JobReference != null &&
                   (item.JobReference.Status == JobStatus.Running ||
                    item.JobReference.Status == JobStatus.Paused);
        }

        private async void OnRunJob(BackupStatusItemModel item)
        {
            if (item == null || item.JobReference == null) return;
            await RunJobAsync(item.JobReference);
        }

        private async Task RunJobAsync(BackupJob job)
        {
            var item = JobStatusItems.FirstOrDefault(i => i.JobId == job.Id);
            if (item != null)
            {
                SubscribeToJob(job, item);
                await job.RunAsync();
            }
        }

        private async void OnRunAllJobs(object param)
        {
            foreach (var job in _jobViewModel.Jobs)
            {
                await RunJobAsync(job);
            }
        }

        private void OnPauseJob(BackupStatusItemModel item)
        {
            if (item == null || item.JobReference == null) return;
            _jobViewModel.PauseCurrentJob(item.JobReference);
        }

        private void OnStopJob(BackupStatusItemModel item)
        {
            if (item == null || item.JobReference == null) return;
            _jobViewModel.CurrentJob = item.JobReference;
            _jobViewModel.StopCurrentJob(item.JobReference);
        }
    }
}