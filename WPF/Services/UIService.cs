using System.Windows;
using Core.Model.Interfaces;
using Core.Utils;

namespace WPF.Services
{
    public class UIService : IUIService
    {
        public void ShowToast(string message, int durationMs = 3000)
        {
            ToastBridge.ShowToast?.Invoke(message, durationMs);
        }
        
        public bool Confirm(string message, string title = "Confirm")
        {
            var result = MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Warning);
            return result == MessageBoxResult.Yes;
        }
        
        public DeleteJobChoice ConfirmDeleteJobWithFiles(string jobName, string targetDir)
        {
            string message = $"Do you also want to delete all backup files at:\n{targetDir}\n\nThis cannot be undone!";
            var result = MessageBox.Show(
                message,
                $"Delete Backup Job: {jobName}",
                MessageBoxButton.YesNoCancel,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Cancel)
                return DeleteJobChoice.Cancel;
            if (result == MessageBoxResult.Yes)
                return DeleteJobChoice.DeleteJobAndFiles;
            return DeleteJobChoice.DeleteJobOnly;
        }
    }
}