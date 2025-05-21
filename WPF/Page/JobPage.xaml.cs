using Microsoft.Win32;
using System.Windows;
using System.Windows.Controls;
using WinForms = System.Windows.Forms;
using Core.ViewModel;

namespace WPF
{
    public partial class JobPage : System.Windows.Controls.Page
    {
        private readonly JobViewModel _vm;

        public JobPage()
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