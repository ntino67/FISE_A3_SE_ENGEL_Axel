using System.Windows;
using System.Windows.Controls;
using Core.Utils;
using Core.ViewModel;
using WPF.Infrastructure;
using WinForms = System.Windows.Forms;

namespace WPF.Pages
{
    public partial class JobSettingsPage : Page
    {
        private readonly JobViewModel _vm;

        public JobSettingsPage()
        {
            InitializeComponent();
            _vm = ViewModelLocator.GetJobViewModel();
        }

        private void OnSetSourcePathClick(object sender, RoutedEventArgs e)
        {
            using var dialog = new WinForms.FolderBrowserDialog();
            if (dialog.ShowDialog() == WinForms.DialogResult.OK)
                _vm.UpdateSourcePath(dialog.SelectedPath);
        }

        private void OnSetTargetPathClick(object sender, RoutedEventArgs e)
        {
            using var dialog = new WinForms.FolderBrowserDialog();
            if (dialog.ShowDialog() == WinForms.DialogResult.OK)
                _vm.UpdateTargetPath(dialog.SelectedPath);
        }

        private void OnToggleEncryptionClick(object sender, RoutedEventArgs e)
        {
            var key = KeyInput.Text;

            if (!string.IsNullOrEmpty(key))
                _vm.ToggleEncryption(key);
            else
                ToastBridge.ShowToast?.Invoke("🔑 Please enter a key first", 3000);
        }
    }
}