using System;
using System.Windows;
using Core.ViewModel;
using WPF.Infrastructure;
using Core.Model.Interfaces;
using Core.Model.Implementations;
using WPF.Services;
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

            string configDirectory = AppDomain.CurrentDomain.BaseDirectory;
            ConfigurationManager = new ConfigurationManager(configDirectory);

            ViewModelLocator.Initialize();

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
                var localizationService = ViewModelLocator.GetLocalizationService();
                localizationService.ChangeLanguage(languageCode);

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de l'initialisation de la langue : {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
