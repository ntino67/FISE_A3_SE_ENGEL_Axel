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
            RunningInstructions.Add(new RunningInstruction(job, instruction));
        }

        public void RemoveFromQueue(string jobId)
        {
            if (string.IsNullOrEmpty(jobId)) return;

            for (int i = RunningInstructions.Count - 1; i >= 0; i--)
            {
                if (RunningInstructions[i].Job.Id == jobId)
                {
                    RunningInstructions.RemoveAt(i);
                }
            }
        }

        public void UpdateProgress(string jobId, float progress)
        {
            foreach (var instruction in RunningInstructions)
            {
                if (instruction.Job.Id == jobId)
                {
                    instruction.Progress = progress;
                    break;
                }
            }
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
        public float Progress { get; set; }

        public RunningInstruction(BackupJob job, Instruction instruction)
        {
            Job = job;
            Instruction = instruction;
            Progress = 0;
        }
    }
}
