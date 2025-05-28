using System;
using Newtonsoft.Json;
using System.Xml.Serialization;

namespace Core.Model
{
    [Serializable]
    public class LogEntry
    {
        [JsonProperty("timestamp")]
        [XmlElement("Timestamp")]
        public DateTime Timestamp { get; set; }

        [JsonProperty("jobName")]
        [XmlElement("JobName")]
        public string JobName { get; set; }

        [JsonProperty("sourcePath")]
        [XmlElement("SourcePath")]
        public string SourcePath { get; set; }

        [JsonProperty("destinationPath")]
        [XmlElement("DestinationPath")]
        public string DestinationPath { get; set; }

        [JsonProperty("fileSize")]
        [XmlElement("FileSize")]
        public long FileSize { get; set; }

        [JsonProperty("transferTimeMs")]
        [XmlElement("TransferTimeMs")]
        public long TransferTimeMs { get; set; }

        [JsonProperty("status")]
        [XmlElement("Status")]
        public string Status { get; set; }

        private long? _encryptionDuration;

        /// <summary>
        /// Time required to encrypt the file (in ms):
        /// 0: no encryption
        /// >0: encryption time (in ms)
        /// <0: error code
        /// </summary>

        [JsonProperty("EncryptionDuration")]
        [XmlElement("EncryptionDuration")]
        public long EncryptionDuration
        {
            get => _encryptionDuration ?? 0;
            set => _encryptionDuration = value;
        }

        [JsonProperty("EncryptedDirectoryPath")]
        [XmlElement("EncryptedDirectoryPath")]
        public string EncryptedDirectoryPath { get; set; }

        // Helper property for XML serialization
        [XmlIgnore]
        [JsonIgnore]
        public bool EncryptionDurationSpecified => _encryptionDuration.HasValue;

        // Helper property for interpreting encryption status
        [XmlIgnore]
        [JsonIgnore]
        public string EncryptionStatusDescription
        {
            get
            {
                if (!_encryptionDuration.HasValue || _encryptionDuration == 0)
                    return "No encryption";
                else if (_encryptionDuration > 0)
                    return $"Encrypted in {_encryptionDuration}ms";
                else
                    return $"Encryption error (code: {_encryptionDuration})";
            }
        }
    }
}
