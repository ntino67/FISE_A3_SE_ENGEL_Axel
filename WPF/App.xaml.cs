using System;
using System.Windows;
using WPF.Infrastructure;
using Core.Model.Interfaces;
using Core.Model.Implementations;
using System.Threading;

namespace WPF
{
    public partial class App : Application
    {
        public IConfigurationManager ConfigurationManager { get; private set; }

        private static Mutex _mutex;

        protected override void OnStartup(StartupEventArgs e)
        {
            bool createdNew;
            _mutex = new Mutex(true, "EasySaveSingleInstance", out createdNew);

            if (!createdNew)
            {
                MessageBox.Show("EasySave is already running.", "Single Instance", MessageBoxButton.OK, MessageBoxImage.Warning);
                Shutdown();
                return;
            }

            base.OnStartup(e);
            ViewModelLocator.Initialize();

            var processMonitor = ViewModelLocator.GetProcessMonitor();
            processMonitor.StartMonitoring();


            string configDirectory = AppDomain.CurrentDomain.BaseDirectory;
            ConfigurationManager = new ConfigurationManager(configDirectory);

            // Initialiser le gestionnaire de transferts de fichiers volumineux avec la valeur de configuration
            Core.Utils.LargeFileTransferManager.Instance.MaxFileSizeKB = ConfigurationManager.GetMaxFileSizeKB();


            

            InitializeLanguage();
        }

        private void InitializeLanguage()
        {
            try
            {
                string languageCode = ConfigurationManager.GetLanguage();
                if (string.IsNullOrEmpty(languageCode))
                {
                    languageCode = "en_US"; // Langue par défaut
                    ConfigurationManager.SaveLanguage(languageCode);
                }

                // Utilisation du service de localisation
                var localizationService = ViewModelLocator.LocalizationService;
                localizationService.ChangeLanguage(languageCode);

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de l'initialisation de la langue : {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
