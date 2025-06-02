using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Core.Model.Interfaces
{
    public interface ISettingsViewModel
    {
        // Properties used for settings UI
        ObservableCollection<string> BlockingApplications { get; set; }
        ObservableCollection<string> EncryptionFileExtensions { get; set; }
        ObservableCollection<string> PriorityExtensions { get; set; }

        // Log paths for display
        string DailyLogFilePath { get; }
        string WarningsLogFilePath { get; }
        string LogsDirectoryPath { get; }
        string StateFilePath { get; }
        string JsonLogFilePath { get; }
        string XmlLogFilePath { get; }

        // Methods for updating the settings
        void AddBlockingApplication(string applicationName);
        void RemoveBlockingApplication(string applicationName);
        void AddEncryptionExtension(string extension);
        void RemoveEncryptionExtension(string extension);
        void SaveSettings();
        void ReloadSettings();
    }
}