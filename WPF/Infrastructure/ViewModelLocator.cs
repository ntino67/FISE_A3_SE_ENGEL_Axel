using System;
using System.IO;
using Core.Model.Implementations;
using Core.Model.Interfaces;
using Core.ViewModel;
using WPF.Services;
using WPF.Utils;
using Core.Utils;

namespace WPF.Infrastructure
{
    public class ViewModelLocator
    {
        private static IBackupService _jobManager;
        private static IConfigurationManager _configManager;
        private static ILogger _logger;
        private static IJobViewModel _jobViewModel;
        private static ISettingsViewModel _settingsViewModel;
        private static IUIService _iuiService;
        private static ILocalizationService _localizationService;
        private static ICommandFactory _commandFactory;
        private static IResourceService _resourceService;
        private static IInstructionHandlerViewModel _instructionHandlerViewModel;
        private static ProcessMonitor _processMonitor;
        private static IProcessChecker _processChecker;

        // Public properties accessible both in WPF and code
        public static IJobViewModel JobViewModel
        {
            get
            {
                if (_jobViewModel == null)
                    throw new InvalidOperationException("ViewModelLocator has not been initialized. Call Initialize() first.");
                return _jobViewModel;
            }
        }
        
        public static IUIService UiService
        {
            get
            {
                if (_iuiService == null)
                    _iuiService = new UIService();
                return _iuiService;
            }
        }
        
        public static ISettingsViewModel SettingsViewModel
        {
            get
            {
                if (_settingsViewModel == null)
                    throw new InvalidOperationException("ViewModelLocator has not been initialized. Call Initialize() first.");
                return _settingsViewModel;
            }
        }
        
        public static ILocalizationService LocalizationService
        {
            get
            {
                if (_localizationService == null)
                {
                    _localizationService = new LocalizationService();
                }
                return _localizationService;
            }
        }

        public static void Initialize()
        {
            if (_configManager != null)
                return;

            string appDataPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "EasySave");

            try
            {
                _configManager = new ConfigurationManager(appDataPath);
                _logger = new Logger(_configManager.GetLogDirectory());
                _iuiService = new UIService();
                _localizationService = new LocalizationService();
                _resourceService = new ResourceService();
                _commandFactory = new WpfCommandFactory();
                _processMonitor = new ProcessMonitor(_configManager, _iuiService, _resourceService, _logger);
                _processChecker = new DefaultProcessChecker();

                _jobManager = new JobManager(_logger, _configManager, _iuiService, _resourceService, _processChecker, _processMonitor);

                _instructionHandlerViewModel = new InstructionHandlerViewModel(_jobManager, _iuiService);
                _jobViewModel = new JobViewModel(_jobManager, _iuiService, _commandFactory, _instructionHandlerViewModel, _configManager);
                _settingsViewModel = new SettingsViewModel(_configManager, _localizationService, _logger, _commandFactory);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to initialize ViewModelLocator", ex);
            }
        }
        
        public static IBackupService GetJobManager() => _jobManager ?? throw new InvalidOperationException("Call Initialize() first.");
        public static IConfigurationManager GetConfigurationManager() => _configManager ?? throw new InvalidOperationException("Call Initialize() first.");
        public static ILogger GetLogger() => _logger ?? throw new InvalidOperationException("Call Initialize() first.");

        public static IResourceService GetResourceService()
        {
            if (_resourceService == null)
            {
                _resourceService = new ResourceService();
            }
            return _resourceService;
        }
        public static ProcessMonitor GetProcessMonitor() => _processMonitor ?? throw new InvalidOperationException("Call Initialize() first.");
    }
}