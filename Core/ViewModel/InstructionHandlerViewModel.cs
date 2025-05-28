using Core.Model;
using Core.Model.Interfaces;
using Core.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

        public void AddToQueue(BackupJob job, Instruction instruction, string key = null)
        {
            if (job == null)
                throw new ArgumentNullException(nameof(job));
            var progress = new Progress<float>(p => job.Progress = p);
            var task = Task.Run(async () =>
            {
                try
                {
                    switch (instruction)
                    {
                        case Instruction.Encrypt:
                            Thread.Sleep(1000); // Here put _jobManager.ExecuteEncryptJob(job.Id, job.Progress, key);
                            break;
                        case Instruction.Decrypt:
                            Thread.Sleep(1000); // Here put _jobManager.ExecuteDecryptJob(job.Id, job.Progress, key);
                            break;
                        case Instruction.Backup:
                            Thread.Sleep(1000); // Here put _jobManager.ExecuteBackupJob(job.Id, job.Progress, key);
                            break;
                    }
                }
                catch (OperationCanceledException)
                {
                    Console.WriteLine($"{instruction} cancelled for {job.Name}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error in {instruction} for {job.Name}: {ex.Message}");
                }
            });

            var runningInstruction = new RunningInstruction
            {
                Job = job,
                Instruction = instruction,
                Task = task
            };

            Application.Current.Dispatcher.Invoke(() => RunningInstructions.Add(runningInstruction));
        }
    }

    public class RunningInstruction
    {
        public BackupJob Job { get; set; }
        public Instruction Instruction { get; set; }
        public Task Task { get; set; }
        public Progress<float> Progress { get; set; } // bindable
    }
}
