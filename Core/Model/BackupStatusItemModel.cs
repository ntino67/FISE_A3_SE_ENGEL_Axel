using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Core.Model
{
    public class BackupStatusItemModel : INotifyPropertyChanged
    {
        private string _jobId;
        private string _jobName;
        private string _instruction;
        private double _progress;
        private string _status;
        private DateTime? _startTime;
        private DateTime? _endTime;
        private long _totalBytes;
        private long _bytesCopied;

        public BackupJob JobReference { get; set; }

        public string JobId
        {
            get => _jobId;
            set { _jobId = value; OnPropertyChanged(); }
        }

        public string JobName
        {
            get => _jobName;
            set { _jobName = value; OnPropertyChanged(); }
        }

        public string Instruction
        {
            get => _instruction;
            set { _instruction = value; OnPropertyChanged(); }
        }

        public double Progress
        {
            get => _progress;
            set
            {
                if (_progress != value)
                {
                    _progress = value;
                    OnPropertyChanged(nameof(Progress));
                }
            }
        }

        public string Status
        {
            get => _status;
            set { _status = value; OnPropertyChanged(); }
        }

        public DateTime? StartTime
        {
            get => _startTime;
            set { _startTime = value; OnPropertyChanged(); }
        }

        public DateTime? EndTime
        {
            get => _endTime;
            set { _endTime = value; OnPropertyChanged(); }
        }

        public long TotalBytes
        {
            get => _totalBytes;
            set
            {
                if (_totalBytes != value)
                {
                    _totalBytes = value;
                    OnPropertyChanged(nameof(TotalBytes));
                    UpdateProgress();
                }
            }
        }

        public long BytesCopied
        {
            get => _bytesCopied;
            set
            {
                if (_bytesCopied != value)
                {
                    _bytesCopied = value;
                    OnPropertyChanged(nameof(BytesCopied));
                    UpdateProgress();
                }
            }
        }

        private void UpdateProgress()
        {
            Progress = (TotalBytes > 0) ? (BytesCopied * 100.0 / TotalBytes) : 0;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}