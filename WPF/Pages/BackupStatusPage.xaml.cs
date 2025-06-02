using System.Windows.Controls;
using Core.Model.Interfaces;
using WPF.Infrastructure;

namespace WPF.Pages
{
    public partial class BackupStatusPage : Page
    {
        private readonly IBackupStatusViewModel _statusVm;

        public BackupStatusPage()
        {
            InitializeComponent();
            _statusVm = ViewModelLocator.BackupStatusViewModel;
            DataContext = ViewModelLocator.JobViewModel;
        }
    }
}