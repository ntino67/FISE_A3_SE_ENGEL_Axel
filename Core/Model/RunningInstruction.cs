using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Core.Utils;

namespace Core.Model
{
    public class RunningInstruction : INotifyPropertyChanged
    {
        private BackupJob _job;
        private Instruction _instructionType;
        private DateTime _startTime;
        private float _progress;

        public BackupJob Job
        {
            get => _job;
            set
            {
                if (_job != value)
                {
                    _job = value;
                    OnPropertyChanged();
                }
            }
        }

        public Instruction InstructionType
        {
            get => _instructionType;
            set
            {
                if (_instructionType != value)
                {
                    _instructionType = value;
                    OnPropertyChanged();
                }
            }
        }

        public DateTime StartTime
        {
            get => _startTime;
            set
            {
                if (_startTime != value)
                {
                    _startTime = value;
                    OnPropertyChanged();
                }
            }
        }

        public float Progress
        {
            get => _progress;
            set
            {
                if (_progress != value)
                {
                    _progress = value;
                    OnPropertyChanged();
                }
            }
        }

        public RunningInstruction(BackupJob job, Instruction instructionType)
        {
            Job = job;
            InstructionType = instructionType;
            StartTime = DateTime.Now;
            Progress = 0;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}