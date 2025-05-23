using System.Windows;
using Core.ViewModel;
using WPF.Infrastructure;
using Core.Model.Interfaces;
using Core.Model.Implementations;
using System;


namespace WPF
{
    public partial class App : Application
    {

        public IConfigurationManager ConfigurationManager { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            string configDirectory = AppDomain.CurrentDomain.BaseDirectory;
            ConfigurationManager = new ConfigurationManager(configDirectory);

            ViewModelLocator.Initialize();
        }
    }
}