using System.Collections.ObjectModel;
using System.ComponentModel;
using Core.Utils;
using Core.ViewModel;

namespace Core.Model.Interfaces
{
    public interface IInstructionHandlerViewModel : INotifyPropertyChanged
    {
        ObservableCollection<RunningInstruction> RunningInstructions { get; set; }

        void AddToQueue(BackupJob job, Instruction instruction);
    }
}