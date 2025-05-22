using System;
using System.IO;
using Core.Model.Implementations;
using Core.Model.Interfaces;
using Core.ViewModel;
using WPF.Services;

namespace WPF.Infrastructure
{
    public static class ViewModelLocator
    {
        private static IBackupService _jobManager;
        private static IConfigurationManager _configManager;
        private static ILogger _logger;
        private static JobViewModel _jobViewModel;
        private static IUIService _iuiService;
        
        public static JobViewModel JobViewModel
        {
            get
            {
                if (_jobViewModel == null)
                    throw new InvalidOperationException("ViewModelLocator has not been initialized. Call Initialize() first.");
                return _jobViewModel;
            }
        }

        public static void Initialize()
        {
            string appDataPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "EasySave");

            _configManager = new ConfigurationManager(appDataPath);
            _logger = new Logger(_configManager.GetLogDirectory());
            _jobManager = new JobManager(_logger, _configManager);
            _iuiService = new UIService();

            _jobViewModel = new JobViewModel(_jobManager, _iuiService);
        }
        
        public static JobViewModel GetJobViewModel() => JobViewModel;
        public static IBackupService GetJobManager() => _jobManager ?? throw new InvalidOperationException("Call Initialize() first.");
        public static IConfigurationManager GetConfigurationManager() => _configManager ?? throw new InvalidOperationException("Call Initialize() first.");
        public static ILogger GetLogger() => _logger ?? throw new InvalidOperationException("Call Initialize() first.");
    }
}