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
        private readonly ILocalizationService _localizationService;
        private ObservableCollection<string> _blockingApplications;
        private string _selectedLanguage;
        private List<KeyValuePair<string, string>> _languageOptions;

        public SettingsViewModel(IConfigurationManager configManager, ILocalizationService localizationService)
        {
            _configManager = configManager;
            _localizationService = localizationService;
            _blockingApplications = new ObservableCollection<string>();
            _languageOptions = _localizationService.GetAvailableLanguages();
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

        public string SelectedLanguage
        {
            get => _selectedLanguage;
            set
            {
                if (_selectedLanguage != value)
                {
                    _selectedLanguage = value;
                    OnPropertyChanged();
                }
            }
        }

        public List<KeyValuePair<string, string>> LanguageOptions => _languageOptions;

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
            _configManager.SaveLanguage(SelectedLanguage);

            // Utilisation du service de localisation
            _localizationService.ChangeLanguage(SelectedLanguage);
        }

        public void ReloadSettings()
        {
            var apps = _configManager.GetBlockingApplications();
            BlockingApplications = new ObservableCollection<string>(apps ?? new List<string>());

            string savedLanguage = _configManager.GetLanguage();
            SelectedLanguage = string.IsNullOrEmpty(savedLanguage) ? "en-US" : savedLanguage;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
