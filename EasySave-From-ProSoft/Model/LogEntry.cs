using System;
using Newtonsoft.Json;

namespace EasySave_From_ProSoft.Model
{
    public class LogEntry
    {
        [JsonProperty("timestamp")]
        public DateTime Timestamp { get; set; }

        [JsonProperty("jobName")]
        public string JobName { get; set; }

        [JsonProperty("sourcePath")]
        public string SourcePath { get; set; }

        [JsonProperty("destinationPath")]
        public string DestinationPath { get; set; }

        [JsonProperty("fileSize")]
        public long FileSize { get; set; }

        [JsonProperty("transferTimeMs")]
        public long TransferTimeMs { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }
    }
}
