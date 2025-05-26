using System;
using System.IO;
using System.Windows;
using System.Windows.Input;
using Core.Model.Implementations;
using Core.Model.Interfaces;
using Core.ViewModel;
using WPF.Services;
using WPF.Utils;

namespace WPF.Infrastructure
{
    public static class ViewModelLocator
    {
        private static IBackupService _jobManager;
        private static IConfigurationManager _configManager;
        private static ILogger _logger;
        private static JobViewModel _jobViewModel;
        private static SettingsViewModel _settingsViewModel;
        private static IUIService _iuiService;
        private static ILocalizationService _localizationService;
        private static ICommandFactory _commandFactory;
        
        public static JobViewModel JobViewModel
        {
            get
            {
                if (_jobViewModel == null)
                    throw new InvalidOperationException("ViewModelLocator has not been initialized. Call Initialize() first.");
                return _jobViewModel;
            }
        }
        public static SettingsViewModel SettingsViewModel
        {
            get
            {
                if (_settingsViewModel == null)
                    throw new InvalidOperationException("ViewModelLocator has not been initialized. Call Initialize() first.");
                return _settingsViewModel;
            }
        }



        public static void Initialize()
        {
            if (_configManager != null) // Déjà initialisé
                return;


            string appDataPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "EasySave");

            try
            {
                _configManager = new ConfigurationManager(appDataPath);
                _logger = new Logger(_configManager.GetLogDirectory());
                _jobManager = new JobManager(_logger, _configManager);
                _iuiService = new UIService();
                _localizationService = new LocalizationService();
                _jobViewModel = new JobViewModel(_jobManager, _iuiService, _commandFactory);
                _settingsViewModel = new SettingsViewModel(_configManager, _localizationService);
            }
            catch (Exception ex)
            {
                // Log l'erreur et initialise au moins les services essentiels
                System.Diagnostics.Debug.WriteLine($"Erreur lors de l'initialisation : {ex.Message}");

                // Garantir que les services critiques sont initialisés
                if (_localizationService == null)
                    _localizationService = new LocalizationService();
            }

        }

        public static JobViewModel GetJobViewModel() => JobViewModel;
        public static SettingsViewModel GetSettingsViewModel() => SettingsViewModel;
        public static IBackupService GetJobManager() => _jobManager ?? throw new InvalidOperationException("Call Initialize() first.");
        public static IConfigurationManager GetConfigurationManager() => _configManager ?? throw new InvalidOperationException("Call Initialize() first.");
        public static ILogger GetLogger() => _logger ?? throw new InvalidOperationException("Call Initialize() first.");
        public static ILocalizationService GetLocalizationService()
        {
            if (_localizationService == null)
            {
                _localizationService = new LocalizationService();
            }
            return _localizationService;
        }
    }
}