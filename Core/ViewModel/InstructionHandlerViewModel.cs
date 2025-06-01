using Core.Model;
using Core.Utils;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using Core.Model.Interfaces;

namespace Core.ViewModel
{
    public class InstructionHandlerViewModel : INotifyPropertyChanged
    {
        private readonly IBackupService _backupService;
        private readonly IUIService _uiService;

        public InstructionHandlerViewModel(IBackupService backupService, IUIService uiService)
        {
            _backupService = backupService ?? throw new ArgumentNullException(nameof(backupService));
            _uiService = uiService ?? throw new ArgumentNullException(nameof(uiService));
            _runningInstructions = new ObservableCollection<RunningInstruction>();
        }

        private ObservableCollection<RunningInstruction> _runningInstructions;

        public ObservableCollection<RunningInstruction> RunningInstructions
        {
            get => _runningInstructions;
            set
            {
                if (_runningInstructions != value)
                {
                    _runningInstructions = value;
                    OnPropertyChanged();
                }
            }
        }

        public void AddToQueue(BackupJob job, Instruction instruction)
        {
            if (job == null) return;
            //If the job is already in the queue just modify instruction
            var existingInstruction = RunningInstructions.FirstOrDefault(i => i.Job.Id == job.Id);
            if (existingInstruction != null) {
                existingInstruction.Instruction = instruction;
                return;
            }
            RunningInstructions.Add(new RunningInstruction(job, instruction));
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class RunningInstruction
    {
        public BackupJob Job { get; set; }
        public Instruction Instruction { get; set; }

        public RunningInstruction(BackupJob job, Instruction instruction)
        {
            Job = job;
            Instruction = instruction;
        }
    }
}
