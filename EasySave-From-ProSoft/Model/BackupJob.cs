using System;
using System.Text.Json.Serialization;
using EasySave_From_ProSoft.Utils;

namespace EasySave_From_ProSoft.Model
{
    public class BackupJob
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("sourceDirectory")]
        public string SourceDirectory { get; set; }

        [JsonPropertyName("targetDirectory")]
        public string TargetDirectory { get; set; }

        [JsonPropertyName("type")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public BackupType Type { get; set; }

        [JsonPropertyName("id")]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [JsonPropertyName("lastRunTime")]
        public DateTime? LastRunTime { get; set; }

        [JsonPropertyName("state")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public JobStatus Status { get; set; } = JobStatus.Ready;

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
