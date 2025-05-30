using Core.Model;
using Core.Model.Interfaces;
using Core.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Core.ViewModel
{
    public class InstructionHandlerViewModel
    {
        private readonly IBackupService _jobManager;
        private readonly IUIService _ui;

        public InstructionHandlerViewModel(IBackupService jobManager, IUIService ui)
        {
            _jobManager = jobManager;
            _ui = ui;
        }

        public ObservableCollection<RunningInstruction> RunningInstructions { get; } = new ObservableCollection<RunningInstruction>();
        public object App { get; private set; }

        public void AddToQueue(BackupJob job, Instruction instruction)
        {
            if (job == null)
                throw new ArgumentNullException(nameof(job));

            var runningInstruction = new RunningInstruction
            {
                Job = job,
                Instruction = instruction,
            };
            //Verify if the job is already in the list and if so , update the instruction
            var existingInstruction = RunningInstructions.FirstOrDefault(ri => ri.Job.Id == job.Id);
            if (existingInstruction != null)
            {
                existingInstruction.Instruction = instruction;
                return; // If the job is already in the list, just update the instruction
            }
            // If the job is not in the list, add it
            Application.Current.Dispatcher.Invoke(() => RunningInstructions.Add(runningInstruction));
        }
    }

    public class RunningInstruction
    {
        public BackupJob Job { get; set; }
        public Instruction Instruction { get; set; }
    }
}
