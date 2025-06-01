using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Core.Utils;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace Core.Model
{
    public class BackupJob : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private string _name;
        private string _sourceDirectory;
        private string _targetDirectory;
        private BackupType _type;
        private DateTime? _lastRunTime;
        private JobStatus _status = JobStatus.Ready;
        private bool _isChecked;
        private bool _isActive;
        private bool _isEncrypted;
        private float _progress;
        private bool _isPriorityJob;
        private DateTime? _startTime;
        private DateTime? _endTime;
        private long _totalBytes;
        private long _bytesCopied;

        // Static property for tracking priority jobs
        private static int _numberOfPriorityJobRunning = 0;
        // Propriété en lecture seule
        public static int NumberOfPriorityJobRunning => _numberOfPriorityJobRunning;

        // Méthodes thread-safe pour incrémenter/décrémenter
        public static void IncrementPriorityJobCount()
        {
            Interlocked.Increment(ref _numberOfPriorityJobRunning);
        }

        public static void DecrementPriorityJobCount()
        {
            Interlocked.Decrement(ref _numberOfPriorityJobRunning);
        }

        // Non-editable property: GUID
        [JsonPropertyName("id")]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [JsonPropertyName("isEncrypted")]
        public bool IsEncrypted
        {
            get { return _isEncrypted; }
            set
            {
                if (_isEncrypted != value)
                {
                    _isEncrypted = value;
                    OnPropertyChanged();
                }
            }
        }

        [JsonPropertyName("name")]
        public string Name
        {
            get => _name;
            set
            {
                if (_name != value)
                {
                    _name = value;
                    OnPropertyChanged();
                }
            }
        }

        [JsonPropertyName("sourceDirectory")]
        public string SourceDirectory
        {
            get => _sourceDirectory;
            set
            {
                if (_sourceDirectory != value)
                {
                    _sourceDirectory = value;
                    OnPropertyChanged();
                }
            }
        }

        [JsonPropertyName("targetDirectory")]
        public string TargetDirectory
        {
            get => _targetDirectory;
            set
            {
                if (_targetDirectory != value)
                {
                    _targetDirectory = value;
                    OnPropertyChanged();
                }
            }
        }

        [JsonPropertyName("type")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public BackupType Type
        {
            get => _type;
            set
            {
                if (_type != value)
                {
                    _type = value;
                    OnPropertyChanged();
                }
            }
        }

        [JsonPropertyName("lastRunTime")]
        public DateTime? LastRunTime
        {
            get => _lastRunTime;
            set
            {
                if (_lastRunTime != value)
                {
                    _lastRunTime = value;
                    OnPropertyChanged();
                }
            }
        }

        [JsonPropertyName("state")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public JobStatus Status
        {
            get => _status;
            set
            {
                if (_status != value)
                {
                    _status = value;
                    OnPropertyChanged();
                }
            }
        }
        
        [JsonIgnore]
        public bool IsChecked
        {
            get => _isChecked;
            set
            {
                if (_isChecked != value)
                {
                    _isChecked = value;
                    OnPropertyChanged();
                }
            }
        }

        [JsonPropertyName("isActive")]
        public bool IsActive
        {
            get => _isActive;
            set
            {
                if (_isActive != value)
                {
                    _isActive = value;
                    OnPropertyChanged();
                }
            }
        }
       
        [JsonPropertyName("progress")]
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

        [JsonPropertyName("isPriorityJob")]
        public bool isPriorityJob
        {
            get => _isPriorityJob;
            set
            {
                if (_isPriorityJob != value)
                {
                    _isPriorityJob = value;
                    OnPropertyChanged();
                }
            }
        }

        [JsonPropertyName("startTime")]
        public DateTime? StartTime
        {
            get => _startTime;
            set
            {
                if (_startTime != value)
                {
                    _startTime = value;
                    OnPropertyChanged(nameof(StartTime));
                }
            }
        }

        [JsonPropertyName("endTime")]
        public DateTime? EndTime
        {
            get => _endTime;
            set
            {
                if (_endTime != value)
                {
                    _endTime = value;
                    OnPropertyChanged(nameof(EndTime));
                }
            }
        }

        public long TotalBytes
        {
            get => _totalBytes;
            private set
            {
                if (_totalBytes != value)
                {
                    _totalBytes = value;
                    OnPropertyChanged(nameof(TotalBytes));
                }
            }
        }

        public long BytesCopied
        {
            get => _bytesCopied;
            private set
            {
                if (_bytesCopied != value)
                {
                    _bytesCopied = value;
                    OnPropertyChanged(nameof(BytesCopied));
                }
            }
        }

        public bool IsValid()
        {
            return !string.IsNullOrEmpty(Name) &&
                   !string.IsNullOrEmpty(SourceDirectory) &&
                   !string.IsNullOrEmpty(TargetDirectory);
        }

        public void Reset()
        {
            SourceDirectory = string.Empty;
            TargetDirectory = string.Empty;
            Type = BackupType.Full;
            Status = JobStatus.Ready;
            LastRunTime = null;
            Progress = 0;
        }

        public event Action<long, long> ProgressChanged; // (bytesCopied, totalBytes)
        public event Action<DateTime> JobStarted;
        public event Action<DateTime> JobEnded;
        public event Action<string> StatusChanged;

        public async Task RunAsync()
        {
            // Set start time and status
            StartTime = DateTime.Now;
            JobStarted?.Invoke(StartTime.Value);
            StatusChanged?.Invoke("Running");

            // Get all files to copy
            var files = Directory.Exists(SourceDirectory)
                ? Directory.GetFiles(SourceDirectory, "*", SearchOption.AllDirectories)
                : new string[0];

            TotalBytes = files.Sum(f => new FileInfo(f).Length);
            BytesCopied = 0;

            // Simulate real copy with delay and update progress
            foreach (var file in files)
            {
                var fileInfo = new FileInfo(file);
                var fileLength = fileInfo.Length;

                // Simulate file copy (replace with real copy logic in production)
                await Task.Run(() =>
                {
                    // Simulate time for copying (e.g., 10ms per MB)
                    int ms = (int)Math.Max(5, fileLength / (1024 * 1024) * 10);
                    Thread.Sleep(ms);
                });

                BytesCopied += fileLength;
                ProgressChanged?.Invoke(BytesCopied, TotalBytes);
            }

            // Set end time and status
            EndTime = DateTime.Now;
            StatusChanged?.Invoke("Completed");
            JobEnded?.Invoke(EndTime.Value);
        }

        private long CalculateTotalSize()
        {
            // Logic to calculate total size of all files in SourceDirectory
            return 0; // Placeholder
        }

        private IEnumerable<string> EnumerateFiles(string directory)
        {
            // Logic to enumerate files in the directory
            return new List<string>(); // Placeholder
        }

        private long CopyFile(string file)
        {
            // Logic to copy file and return its size
            return new FileInfo(file).Length; // Placeholder
        }
    }
}
