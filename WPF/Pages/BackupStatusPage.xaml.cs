using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
            RunCommand = new RelayCommand<BackupStatusItemModel>(OnRunJob);
            PauseCommand = new RelayCommand<BackupStatusItemModel>(OnPauseJob);
            StopCommand = new RelayCommand<BackupStatusItemModel>(OnStopJob);

            // Initialize with jobs from JobViewModel
            LoadJobsFromViewModel();
        }

        private void LoadJobsFromViewModel()
        {
            JobStatusItems = new ObservableCollection<BackupStatusItemModel>();

            // Créer un BackupStatusItemModel pour chaque job dans le ViewModel
            foreach (var job in _vm.Jobs)
            {
                JobStatusItems.Add(new BackupStatusItemModel
                {
                    JobName = job.Name,
                    Instruction = job.Type.ToString(),
                    Progress = 0,
                    Status = "Ready",
                    StartTime = null,
                    EndTime = job.LastRunTime
                });
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

        private async void OnRunJob(BackupStatusItemModel item)
        {
            if (item == null) return;

            // Set status to running and start time
            item.Status = "Running";
            item.StartTime = DateTime.Now;
            item.EndTime = null;

            // Simulate running a backup job with progress updates
            for (int i = 0; i <= 100; i += 5)
            {
                item.Progress = i;
                await Task.Delay(100); // Simulating work
            }

            // Set completed status and end time
            item.Status = "Success";
            item.EndTime = DateTime.Now;
        }

        private void OnPauseJob(BackupStatusItemModel item)
        {
            if (item == null) return;

            // Toggle between paused and running state
            if (item.Status == "Running")
            {
                item.Status = "Paused";
                // Store the pause time if needed
            }
            else if (item.Status == "Paused")
            {
                item.Status = "Running";
                // In a real app, you'd resume the job here
            }
        }

        private void OnStopJob(BackupStatusItemModel item)
        {
            if (item == null) return;

            // Stop the job and set end time
            item.Status = "Stopped";
            item.EndTime = DateTime.Now;
            // In a real app, you'd stop the job process here
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
        private string _jobName;
        private string _instruction;
        private double _progress;
        private string _status;
        private DateTime? _startTime;
        private DateTime? _endTime;

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