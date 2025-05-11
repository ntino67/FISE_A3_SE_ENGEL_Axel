using System;
using System.IO;
using EasySave_From_ProSoft.Model.Implementations;
using EasySave_From_ProSoft.Model.Interfaces;

namespace EasySave_From_ProSoft.ViewModel
{
    public static class ViewModelLocator
    {
        private static IBackupService _jobManager;
        private static IConfigurationManager _configManager;
        private static ILogger _logger;
        private static JobViewModel _jobViewModel;

        public static void Initialize()
        {
            string appDataPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "EasySave");

            _configManager = new ConfigurationManager(appDataPath);
            _logger = new Logger(_configManager.GetLogDirectory());
            _jobManager = new JobManager(_logger, _configManager);

            _jobViewModel = new JobViewModel(_jobManager);
        }

        public static JobViewModel GetJobViewModel()
        {
            if (_jobViewModel == null)
                throw new InvalidOperationException("ViewModelLocator n'a pas été initialisé. Appelez Initialize() avant d'utiliser GetJobViewModel().");

            return _jobViewModel;
        }

        public static IBackupService GetJobManager()
        {
            if (_jobManager == null)
                throw new InvalidOperationException("ViewModelLocator n'a pas été initialisé. Appelez Initialize() avant d'utiliser GetJobManager().");

            return _jobManager;
        }

        public static IConfigurationManager GetConfigManager()
        {
            if (_configManager == null)
                throw new InvalidOperationException("ViewModelLocator n'a pas été initialisé. Appelez Initialize() avant d'utiliser GetConfigManager().");

            return _configManager;
        }

        public static ILogger GetLogger()
        {
            if (_logger == null)
                throw new InvalidOperationException("ViewModelLocator n'a pas été initialisé. Appelez Initialize() avant d'utiliser GetLogger().");

            return _logger;
        }
    }
}
