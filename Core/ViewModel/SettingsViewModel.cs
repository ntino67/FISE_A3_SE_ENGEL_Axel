using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Core.Model.Interfaces;

namespace Core.ViewModel
{
    public class SettingsViewModel : INotifyPropertyChanged
    {
        private readonly IConfigurationManager _configManager;
        private ObservableCollection<string> _blockingApplications;

        public SettingsViewModel(IConfigurationManager configManager)
        {
            _configManager = configManager;
            _blockingApplications = new ObservableCollection<string>(); 
            ReloadSettings();
        }

        public ObservableCollection<string> BlockingApplications
        {
            get => _blockingApplications;
            set
            {
                if (_blockingApplications != value)
                {
                    _blockingApplications = value;
                    OnPropertyChanged();
                }
            }
        }

        public void AddBlockingApplication(string applicationName)
        {
            if (!string.IsNullOrWhiteSpace(applicationName) && !BlockingApplications.Contains(applicationName))
            {
                BlockingApplications.Add(applicationName);
                OnPropertyChanged(nameof(BlockingApplications));
            }
        }

        public void RemoveBlockingApplication(string applicationName)
        {
            if (BlockingApplications.Contains(applicationName))
            {
                BlockingApplications.Remove(applicationName);
                OnPropertyChanged(nameof(BlockingApplications));
            }
        }

        public void SaveSettings()
        {
            _configManager.SaveBlockingApplications(new List<string>(BlockingApplications));
        }

        public void ReloadSettings()
        {
            var apps = _configManager.GetBlockingApplications();
            BlockingApplications = new ObservableCollection<string>(apps ?? new List<string>());
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
