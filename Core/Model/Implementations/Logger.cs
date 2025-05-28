using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using Newtonsoft.Json;
using Core.Model.Interfaces;

namespace Core.Model.Implementations
{
    public class Logger : ILogger
    {
        private readonly string _logDirectory;
        private readonly XmlSerializer _xmlSerializer;

        public Logger(string logDirectory)
        {
            _logDirectory = logDirectory;
            Directory.CreateDirectory(logDirectory);
            _xmlSerializer = new XmlSerializer(typeof(List<LogEntry>), new XmlRootAttribute("LogEntries"));

        }
        public void LogBackupOperation(string jobName, string sourcePath, string destinationPath, long fileSize, long transferTimeMs, string status)
        {
            LogEntry logEntry = new LogEntry
            {
                Timestamp = DateTime.Now,
                JobName = jobName,
                SourcePath = sourcePath,
                DestinationPath = destinationPath,
                FileSize = fileSize,
                TransferTimeMs = transferTimeMs,
                Status = status
            };

            WriteToLogFile(logEntry);
        }

        public void LogBackupStart(BackupJob job)
        {
            LogEntry logEntry = new LogEntry
            {
                Timestamp = DateTime.Now,
                JobName = job.Name,
                SourcePath = job.SourceDirectory,
                DestinationPath = job.TargetDirectory,
                FileSize = 0,
                TransferTimeMs = 0,
                Status = "STARTED"
            };

            WriteToLogFile(logEntry);
        }

        public void LogBackupEnd(BackupJob job, bool success, TimeSpan duration)
        {
            LogEntry logEntry = new LogEntry
            {
                Timestamp = DateTime.Now,
                JobName = job.Name,
                SourcePath = job.SourceDirectory,
                DestinationPath = job.TargetDirectory,
                FileSize = 0,
                TransferTimeMs = (long)duration.TotalMilliseconds,
                Status = success ? "COMPLETED" : "FAILED"
            };

            WriteToLogFile(logEntry);
        }

        public void LogEncryptionStart(BackupJob job)
        {
            LogEntry logEntry = new LogEntry
            {
                Timestamp = DateTime.Now,
                JobName = job.Name,
                EncryptedDirectoryPath = job.TargetDirectory,
                EncryptionDuration = 0,
                Status = "ENCRYPTION_STARTED"
            };
            WriteToLogFile(logEntry);
        }
        public void LogEncryptionEnd(BackupJob job, bool success, long duration)
        {
            long encryptionDuration = success ? duration : -1;

            LogEntry logEntry = new LogEntry
            {
                Timestamp = DateTime.Now,
                JobName = job.Name,
                EncryptedDirectoryPath = job.TargetDirectory,
                EncryptionDuration = encryptionDuration,
                Status = success ? "ENCRYPTION_COMPLETED" : "ENCRYPTION_FAILED"
            }; 
            WriteToLogFile(logEntry);
        }
        public List<LogEntry> GetTodayLogs()
        {
            string logFilePath = GetLogFilePath(DateTime.Now);

            if (!File.Exists(logFilePath))
                return new List<LogEntry>();

            string json = File.ReadAllText(logFilePath);
            try
            {
                return JsonConvert.DeserializeObject<List<LogEntry>>(json) ?? new List<LogEntry>();
            }
            catch
            {
                return new List<LogEntry>();
            }
        }

        public string GetLogFilePath(DateTime date)
        {
            return GetJsonLogFilePath(date);
        }

        public string GetJsonLogFilePath(DateTime date)
        {
            return Path.Combine(_logDirectory, $"EasySave_Log_{date:yyyyMMdd}.json");
        }

        public string GetXmlLogFilePath(DateTime date)
        {
            return Path.Combine(_logDirectory, $"EasySave_Log_{date:yyyyMMdd}.xml");
        }

        private void WriteToLogFile(LogEntry entry)
        {
            // Write to JSON file
            WriteToJsonLogFile(entry);

            // Write to XML file
            WriteToXmlLogFile(entry);
        }

        private void WriteToJsonLogFile(LogEntry entry)
        {
            string logFilePath = GetJsonLogFilePath(DateTime.Now);

            List<LogEntry> existingEntries = new List<LogEntry>();
            if (File.Exists(logFilePath))
            {
                string json = File.ReadAllText(logFilePath);
                try
                {
                    existingEntries = JsonConvert.DeserializeObject<List<LogEntry>>(json) ?? new List<LogEntry>();
                }
                catch
                {
                    // En cas d'erreur, on continue avec une liste vide
                }
            }

            existingEntries.Add(entry);

            string updatedJson = JsonConvert.SerializeObject(existingEntries, Formatting.Indented);
            File.WriteAllText(logFilePath, updatedJson);
        }

        private void WriteToXmlLogFile(LogEntry entry)
        {
            string logFilePath = GetXmlLogFilePath(DateTime.Now);

            List<LogEntry> existingEntries = new List<LogEntry>();
            if (File.Exists(logFilePath))
            {
                try
                {
                    using (FileStream stream = new FileStream(logFilePath, FileMode.Open))
                    {
                        existingEntries = (List<LogEntry>)_xmlSerializer.Deserialize(stream);
                    }
                }
                catch
                {
                    // En cas d'erreur, on continue avec une liste vide
                }
            }

            existingEntries.Add(entry);

            try
            {
                using (FileStream stream = new FileStream(logFilePath, FileMode.Create))
                {
                    _xmlSerializer.Serialize(stream, existingEntries);
                }
            }
            catch (Exception ex)
            {
                LogWarning($"Error writing to XML log file: {ex.Message}");
            }
        }

        public void LogWarning(string message)
        {
            string logMessage = $"[WARNING] [{DateTime.Now}] {message}";
            LogToFile("warnings.log", logMessage);
            Console.WriteLine(logMessage);
        }

        private void LogToFile(string fileName, string message)
        {
            try
            {
                string logFilePath = Path.Combine(_logDirectory, fileName);
                File.AppendAllText(logFilePath, message + Environment.NewLine);
            }
            catch
            {
                // La journalisation ne doit pas provoquer d'erreur dans l'application
            }
        }

    }
}

