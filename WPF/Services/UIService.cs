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
    }
}