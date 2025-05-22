using Microsoft.Win32;
using System.Windows;
using System.Windows.Controls;
using WinForms = System.Windows.Forms;
using Core.ViewModel;

namespace WPF.Pages
{
<<<<<<< HEAD:WPF/Page/JobPage.xaml.cs
    /// <summary>
    /// Logique d'interaction pour JobPage.xaml
    /// </summary>
    public partial class JobPage : System.Windows.Controls.Page
=======
    public partial class JobSettingsPage : System.Windows.Controls.Page
>>>>>>> 3d1f2a1a380d8e46a6b053e9e7300b45187f5f4f:WPF/Pages/JobSettingsPage.xaml.cs
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

        private void OnDeleteJobClick(object sender, RoutedEventArgs e)
        {
            if (_vm.CurrentJob != null)
                _vm.DeleteJob(_vm.CurrentJob.Id);
        }

        private void OnToggleEncryptionClick(object sender, RoutedEventArgs e)
        {
            string key = KeyInput.Text;
            if (!string.IsNullOrEmpty(key))
            {
                // XOR encryption logic (assume implemented elsewhere)
                _vm.ToggleEncryption(key);
            }
            else
            {
                System.Windows.MessageBox.Show("Please enter a key first.");
            }
        }
    }
}