using System;
using System.Windows;
using System.Windows.Controls;
using Core.Utils;
using WPF.Infrastructure;
using WinForms = System.Windows.Forms;
using Core.Model.Interfaces;

namespace WPF.Pages
{
    public partial class JobSettingsPage : Page
    {
        private readonly IJobViewModel _vm;

        public JobSettingsPage()
        {
            InitializeComponent();
            _vm = ViewModelLocator.JobViewModel;
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
    }
}