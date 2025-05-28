using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using Core.Utils;

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

        [JsonIgnore]
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

        [JsonIgnore]
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
        }
    }
}
