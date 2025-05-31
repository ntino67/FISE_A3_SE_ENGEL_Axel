using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Core.Model;
using Core.Model.Interfaces;
using Core.Utils;
using Core.ViewModel;
using WPF.Infrastructure;

namespace WPF.Pages
{
    public partial class BackupStatusPage : Page, INotifyPropertyChanged
    {
        private ObservableCollection<BackupStatusItemModel> _jobStatusItems;
        private readonly JobViewModel _vm;

        public BackupStatusPage()
        {
            // Required for the ProgressToWidthConverter
            this.Resources.Add("ProgressToWidthConverter", new ProgressToWidthConverter());

            InitializeComponent();
            DataContext = this;

            // Récupérer l'instance du JobViewModel
            _vm = ViewModelLocator.JobViewModel;

            // Initialize commands
            RunCommand = new RelayCommand<BackupStatusItemModel>(OnRunJob, CanRunJob);
            PauseCommand = new RelayCommand<BackupStatusItemModel>(OnPauseJob, CanPauseJob);
            StopCommand = new RelayCommand<BackupStatusItemModel>(OnStopJob, CanStopJob);
            RunAllCommand = new RelayCommand<object>(OnRunAllJobs, _ => _jobStatusItems?.Any() == true);

            // Initialize with jobs from JobViewModel
            LoadJobsFromViewModel();

            // Subscribe to job changes in the JobViewModel
            _vm.PropertyChanged += JobViewModel_PropertyChanged;
        }

        private bool CanRunJob(BackupStatusItemModel item)
        {
            // Uniquement activé si le job est prêt ou en pause et a des chemins définis
            if (item?.JobReference == null)
                return false;

            // Vérifier si le job a des chemins source et cible définis
            if (string.IsNullOrWhiteSpace(item.JobReference.SourceDirectory) ||
                string.IsNullOrWhiteSpace(item.JobReference.TargetDirectory))
                return false;

            return item.JobReference.Status == JobStatus.Ready ||
                   item.JobReference.Status == JobStatus.Paused ||
                   item.JobReference.Status == JobStatus.Completed ||
                   item.JobReference.Status == JobStatus.Failed ||
                   item.JobReference.Status == JobStatus.Stopped;
        }

        private bool CanPauseJob(BackupStatusItemModel item)
        {
            // Uniquement activé si le job est en cours d'exécution
            return item?.JobReference != null && item.JobReference.Status == JobStatus.Running;
        }

        private bool CanStopJob(BackupStatusItemModel item)
        {
            // Uniquement activé si le job est en cours d'exécution ou en pause
            return item?.JobReference != null &&
                   (item.JobReference.Status == JobStatus.Running ||
                    item.JobReference.Status == JobStatus.Paused);
        }

        private void JobViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(_vm.Jobs))
            {
                // Reload jobs when the list changes
                LoadJobsFromViewModel();
            }
        }

        private void LoadJobsFromViewModel()
        {
            JobStatusItems = new ObservableCollection<BackupStatusItemModel>();

            // Créer un BackupStatusItemModel pour chaque job dans le ViewModel
            foreach (var job in _vm.Jobs)
            {
                // Vérifier si le job a des chemins source et cible définis
                if (string.IsNullOrWhiteSpace(job.SourceDirectory) ||
                    string.IsNullOrWhiteSpace(job.TargetDirectory))
                {
                    job.Status = JobStatus.Failed;
                }

                // Use the Backup instruction by default
                JobStatusItems.Add(new BackupStatusItemModel
                {
                    JobId = job.Id,
                    JobName = job.Name,
                    Instruction = Instruction.Backup.ToString(), // Use Backup as default instruction
                    Progress = (double)job.Progress,
                    Status = string.IsNullOrWhiteSpace(job.SourceDirectory) ||
                             string.IsNullOrWhiteSpace(job.TargetDirectory)
                                ? "NotReady"
                                : job.Status.ToString(),
                    StartTime = null,
                    EndTime = job.LastRunTime,
                    JobReference = job
                });

                // Subscribe to job property changes to update UI
                job.PropertyChanged += Job_PropertyChanged;
            }
        }

        private void Job_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (sender is BackupJob job)
            {
                var statusItem = JobStatusItems.FirstOrDefault(item => item.JobId == job.Id);
                if (statusItem != null)
                {
                    if (e.PropertyName == nameof(BackupJob.Progress))
                    {
                        statusItem.Progress = job.Progress;
                    }
                    else if (e.PropertyName == nameof(BackupJob.Status))
                    {
                        // Vérifier si le job a des chemins source et cible définis avant de mettre à jour le statut
                        if (string.IsNullOrWhiteSpace(job.SourceDirectory) ||
                            string.IsNullOrWhiteSpace(job.TargetDirectory))
                        {
                            statusItem.Status = "NotReady";
                        }
                        else
                        {
                            statusItem.Status = job.Status.ToString();
                        }

                        // Update start/end times
                        if (job.Status == JobStatus.Running && statusItem.StartTime == null)
                        {
                            statusItem.StartTime = DateTime.Now;
                        }
                        else if ((job.Status == JobStatus.Completed ||
                                 job.Status == JobStatus.Failed ||
                                 job.Status == JobStatus.Stopped ||
                                 job.Status == JobStatus.Canceled) &&
                                 statusItem.EndTime == null)
                        {
                            statusItem.EndTime = DateTime.Now;
                            job.LastRunTime = statusItem.EndTime;
                        }

                        // Force CommandManager to reevaluate CanExecute states
                        CommandManager.InvalidateRequerySuggested();
                    }
                    else if (e.PropertyName == nameof(BackupJob.SourceDirectory) ||
                             e.PropertyName == nameof(BackupJob.TargetDirectory))
                    {
                        // Mettre à jour l'état "NotReady" si les chemins ne sont pas définis
                        if (string.IsNullOrWhiteSpace(job.SourceDirectory) ||
                            string.IsNullOrWhiteSpace(job.TargetDirectory))
                        {
                            statusItem.Status = "NotReady";
                            CommandManager.InvalidateRequerySuggested();
                        }
                        else if (statusItem.Status == "NotReady")
                        {
                            statusItem.Status = JobStatus.Ready.ToString();
                            CommandManager.InvalidateRequerySuggested();
                        }
                    }
                }
            }
        }

        public ObservableCollection<BackupStatusItemModel> JobStatusItems
        {
            get => _jobStatusItems;
            set
            {
                _jobStatusItems = value;
                OnPropertyChanged();
            }
        }

        public ICommand RunCommand { get; }
        public ICommand PauseCommand { get; }
        public ICommand StopCommand { get; }
        public ICommand RunAllCommand { get; }

        private async void OnRunJob(BackupStatusItemModel item)
        {
            if (item == null || item.JobReference == null) return;

            try
            {
                // Si le job est en pause, on le reprend
                if (item.JobReference.Status == JobStatus.Paused)
                {
                    _vm.ResumeCurrentJob(item.JobReference);
                    return;
                }

                // Set status to running and start time
                item.Status = JobStatus.Running.ToString();
                item.StartTime = DateTime.Now;
                item.EndTime = null;

                // Update the instruction to Backup
                item.Instruction = Instruction.Backup.ToString();

                // Execute the actual job using JobViewModel
                await _vm.ExecuteCurrentJob(item.JobReference);

                // Force CommandManager to reevaluate CanExecute states
                CommandManager.InvalidateRequerySuggested();
            }
            catch (Exception ex)
            {
                // Handle error
                item.Status = JobStatus.Failed.ToString();
                item.EndTime = DateTime.Now;
                MessageBox.Show($"Error running job: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void OnRunAllJobs(object param)
        {
            try
            {
                // Filtrer pour ne lancer que les jobs qui ont des chemins définis
                var validJobs = JobStatusItems.Where(item =>
                    item.JobReference != null &&
                    !string.IsNullOrWhiteSpace(item.JobReference.SourceDirectory) &&
                    !string.IsNullOrWhiteSpace(item.JobReference.TargetDirectory)).ToList();

                if (!validJobs.Any())
                {
                    MessageBox.Show("No valid jobs to run. Please ensure jobs have source and target paths defined.",
                                    "No Valid Jobs",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Information);
                    return;
                }

                foreach (var item in validJobs)
                {
                    // Marquer comme en cours d'exécution
                    item.Status = JobStatus.Running.ToString();
                    item.StartTime = DateTime.Now;
                    item.EndTime = null;
                    item.Instruction = Instruction.Backup.ToString();

                    // Rafraîchir l'interface après chaque mise à jour
                    CommandManager.InvalidateRequerySuggested();
                }

                // Exécuter tous les jobs valides
                await _vm.ExecuteAllJobs();

                // Force CommandManager to reevaluate CanExecute states
                CommandManager.InvalidateRequerySuggested();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error running all jobs: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OnPauseJob(BackupStatusItemModel item)
        {
            if (item == null || item.JobReference == null) return;

            try
            {
                // Mise en pause du job
                _vm.PauseCurrentJob(item.JobReference);

                // Force CommandManager to reevaluate CanExecute states
                CommandManager.InvalidateRequerySuggested();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error pausing job: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OnStopJob(BackupStatusItemModel item)
        {
            if (item == null || item.JobReference == null) return;

            try
            {
                // Stop the job
                _vm.CurrentJob = item.JobReference; // Set as current job first
                _vm.StopCurrentJob();
                item.EndTime = DateTime.Now;

                // Force CommandManager to reevaluate CanExecute states
                CommandManager.InvalidateRequerySuggested();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error stopping job: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public async void ToggleEncryption(BackupStatusItemModel item)
        {
            if (item == null || item.JobReference == null) return;

            try
            {
                // Set the current job in the view model
                _vm.CurrentJob = item.JobReference;

                // Check if we're going to encrypt or decrypt
                bool isEncrypt = !item.JobReference.IsEncrypted; // Assuming this property exists

                // Update the instruction
                item.Instruction = isEncrypt ? Instruction.Encrypt.ToString() : Instruction.Decrypt.ToString();

                // Execute encryption/decryption
                await Task.Run(() => _vm.ToggleEncryption(item.JobReference, _vm.EncryptionKey));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error toggling encryption: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #region INotifyPropertyChanged Implementation

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }

    // Renommé la classe pour éviter l'ambiguïté
    public class BackupStatusItemModel : INotifyPropertyChanged
    {
        private string _jobId;
        private string _jobName;
        private string _instruction;
        private double _progress;
        private string _status;
        private DateTime? _startTime;
        private DateTime? _endTime;

        // Reference to the actual BackupJob
        public BackupJob JobReference { get; set; }

        public string JobId
        {
            get => _jobId;
            set
            {
                _jobId = value;
                OnPropertyChanged();
            }
        }

        public string JobName
        {
            get => _jobName;
            set
            {
                _jobName = value;
                OnPropertyChanged();
            }
        }

        public string Instruction
        {
            get => _instruction;
            set
            {
                _instruction = value;
                OnPropertyChanged();
            }
        }

        public double Progress
        {
            get => _progress;
            set
            {
                _progress = value;
                OnPropertyChanged();
            }
        }

        public string Status
        {
            get => _status;
            set
            {
                _status = value;
                OnPropertyChanged();
            }
        }

        public DateTime? StartTime
        {
            get => _startTime;
            set
            {
                _startTime = value;
                OnPropertyChanged();
            }
        }

        public DateTime? EndTime
        {
            get => _endTime;
            set
            {
                _endTime = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class ProgressToWidthConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (values.Length < 4 ||
                !(values[0] is double value) ||
                !(values[1] is double minimum) ||
                !(values[2] is double maximum) ||
                !(values[3] is double actualWidth))
                return 0;

            if (maximum - minimum == 0)
                return 0;

            return ((value - minimum) / (maximum - minimum)) * actualWidth;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class RelayCommand<T> : ICommand
    {
        private readonly Action<T> _execute;
        private readonly Predicate<T> _canExecute;

        public RelayCommand(Action<T> execute, Predicate<T> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute == null || _canExecute((T)parameter);
        }

        public void Execute(object parameter)
        {
            _execute((T)parameter);
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
    }
}