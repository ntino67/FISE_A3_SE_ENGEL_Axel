using System;
using System.Text.Json.Serialization;

namespace EasySave_From_ProSoft.Model
{
    public class LogEntry
    {
        [JsonPropertyName("timestamp")]
        public DateTime Timestamp { get; set; }

        [JsonPropertyName("jobName")]
        public string JobName { get; set; }

        [JsonPropertyName("sourcePath")]
        public string SourcePath { get; set; }

        [JsonPropertyName("destinationPath")]
        public string DestinationPath { get; set; }

        [JsonPropertyName("fileSize")]
        public long FileSize { get; set; }

        [JsonPropertyName("transferTimeMs")]
        public long TransferTimeMs { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; }
    }
}
