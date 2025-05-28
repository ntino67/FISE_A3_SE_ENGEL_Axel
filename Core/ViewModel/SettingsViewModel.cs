using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using Core.Model.Interfaces;

namespace Core.ViewModel
{
    public class SettingsViewModel : INotifyPropertyChanged
    {
        private readonly IConfigurationManager _configManager;
        private readonly ILocalizationService _localizationService;
        private readonly ILogger _logger;
        private ObservableCollection<string> _blockingApplications;
        private string _selectedLanguage;
        private List<KeyValuePair<string, string>> _languageOptions;
        private ObservableCollection<string> _encryptionFileExtensions;
        private string _encryptionWildcard;
  

        public ObservableCollection<string> EncryptionFileExtensions
        {
            get => _encryptionFileExtensions;
            set
            {
                if (_encryptionFileExtensions != value)
                {
                    _encryptionFileExtensions = value;
                    OnPropertyChanged();
                }
            }
        }

        public string EncryptionWildcard
        {
            get => _encryptionWildcard;
            set
            {
                if (_encryptionWildcard != value)
                {
                    _encryptionWildcard = value;
                    OnPropertyChanged();
                }
            }
        }

        public string DailyLogFilePath => _logger.GetLogFilePath(DateTime.Now);
        public string WarningsLogFilePath => Path.Combine(_configManager.GetLogDirectory(), "warnings.log");
        public string LogsDirectoryPath => _configManager.GetLogDirectory();

        public string StateFilePath => _configManager.GetStateFilePath();

        public SettingsViewModel(IConfigurationManager configManager, ILocalizationService localizationService, ILogger logger)
        {
            _configManager = configManager;
            _localizationService = localizationService;
            _logger = logger;
            _blockingApplications = new ObservableCollection<string>();
            _encryptionFileExtensions = new ObservableCollection<string>();
            _languageOptions = _localizationService.GetAvailableLanguages();
            ReloadSettings();
        }

        public void AddEncryptionExtension(string extension)
        {
            if (string.IsNullOrWhiteSpace(extension))
                return;

            // S'assurer que l'extension commence par un point
            string formattedExtension = extension.StartsWith(".") ? extension.ToLower() : "." + extension.ToLower();

            if (!EncryptionFileExtensions.Contains(formattedExtension))
            {
                EncryptionFileExtensions.Add(formattedExtension);
                OnPropertyChanged(nameof(EncryptionFileExtensions));
            }
        }

        public void RemoveEncryptionExtension(string extension)
        {
            if (EncryptionFileExtensions.Contains(extension))
            {
                EncryptionFileExtensions.Remove(extension);
                OnPropertyChanged(nameof(EncryptionFileExtensions));
            }
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
            _configManager.SaveEncryptionFileExtensions(new List<string>(EncryptionFileExtensions));
            _configManager.SaveEncryptionWildcard(EncryptionWildcard);
            _localizationService.ChangeLanguage(SelectedLanguage);
        }

        public void ReloadSettings()
        {
            var apps = _configManager.GetBlockingApplications();
            BlockingApplications = new ObservableCollection<string>(apps ?? new List<string>());

            var extensions = _configManager.GetEncryptionFileExtensions();
            EncryptionFileExtensions = new ObservableCollection<string>(extensions ?? new List<string>());

            EncryptionWildcard = _configManager.GetEncryptionWildcard();

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
