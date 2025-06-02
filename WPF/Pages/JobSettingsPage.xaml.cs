using System;
using System.Windows;
using System.Windows.Controls;
using Core.Utils;
using Core.ViewModel;
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

        private void TaskProgressBar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            // Progress bar value change handler
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            // Cette méthode n'est plus utilisée car le binding Command est utilisé à la place
            // Si le XAML contient encore cette référence, vous pourriez la laisser comme wrapper
            // mais en réalité elle n'est pas appelée car le binding Command prend le dessus
            if (_vm.CurrentJob == null || string.IsNullOrWhiteSpace(_vm.EncryptionKey))
                return;

            // Le statut actuel du bouton détermine l'action à effectuer
            var currentStatus = _vm.EncryptionStatus;

            // Mise à jour du statut du job
            _vm.CurrentJob.Status = JobStatus.Running;
            _vm.CurrentJob.LastRunTime = DateTime.Now; // Commencer à suivre le temps

            // Exécution de la commande pour mettre à jour à la fois le job et la table de statut
            if (_vm.ToggleEncryptionCommand.CanExecute(_vm.CurrentJob))
            {
                _vm.ToggleEncryptionCommand.Execute(_vm.CurrentJob);
            }
        }
    }
}