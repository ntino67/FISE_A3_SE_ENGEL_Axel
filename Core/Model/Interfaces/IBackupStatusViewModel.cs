using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Core.Model.Interfaces
{
    public interface IBackupStatusViewModel
    {
        ObservableCollection<BackupStatusItemModel> JobStatusItems { get; }

        ICommand RunCommand { get; }
        ICommand PauseCommand { get; }
        ICommand StopCommand { get; }
        ICommand RunAllCommand { get; }
    }
}