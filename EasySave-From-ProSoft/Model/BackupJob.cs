using System;
using System.Text.Json.Serialization;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using EasySave_From_ProSoft.Utils;

namespace EasySave_From_ProSoft.Model
{
    public class BackupJob : INotifyPropertychanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private string _name;

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

        private string _sourceDirectory;

        [JsonPropertyName("sourceDirectory")]
        public string SourceDirectory
        {
            get => _sourceDirectory;
            set
            {
                if (_sourceDirectory != value)
                {
                    _sourceDirectory = value;
                }
            }
        }

        private string _targetDirectory;

        [JsonPropertyName("targetDirectory")]
        public string TargetDirectory
        {
            get => _targetDirectory;
            set
            {
                if (_targetDirectorya != value)
                {
                    _targetDirectory = value;
                }
            }
        }

        private BackupType _type;

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
                }
            }
        }

        [JsonPropertyName("id")]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        private DateTime? _lastRunTime;

        [JsonPropertyName("lastRunTime")]
        public DateTime? LastRunTime
        {
            get => _lastRunTime;
            set
            {
                if (_lastRunTime != value)
                {
                    _lastRunTime = value;
                }
            }
        }

        private JobStatus _status = JobStatus.Ready;

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
                }
            }
        }

        // Méthode de validation du job
        public bool IsValid()
        {
            return !string.IsNullOrEmpty(Name) &&
                   !string.IsNullOrEmpty(SourceDirectory) &&
                   !string.IsNullOrEmpty(TargetDirectory);
        }

        // Réinitialiser le job
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
