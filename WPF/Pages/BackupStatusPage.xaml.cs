using System.Windows.Controls;
using WPF.Infrastructure;

namespace WPF.Pages
{
    public partial class BackupStatusPage : Page
    {
        public BackupStatusPage()
        {
            InitializeComponent();
            DataContext = ViewModelLocator.JobViewModel;
        }
    }
}