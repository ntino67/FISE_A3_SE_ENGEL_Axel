using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using Newtonsoft.Json;
using Core.Model.Interfaces;
using System.Threading;

namespace Core.Model.Implementations
{
    public class Logger : ILogger
    {
        private readonly string _logDirectory;
        private readonly XmlSerializer _xmlSerializer;
        private readonly object _jsonLogLock = new object(); // Verrou pour les fichiers JSON
        private readonly object _xmlLogLock = new object();  // Verrou pour les fichiers XML
        private readonly object _warningLogLock = new object(); // Verrou pour le fichier warnings.log

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

            lock (_jsonLogLock) // Verrouiller pendant la lecture du fichier JSON
            {
                if (!File.Exists(logFilePath))
                    return new List<LogEntry>();

                try
                {
                    using (FileStream fs = new FileStream(logFilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                    using (StreamReader reader = new StreamReader(fs))
                    {
                        string json = reader.ReadToEnd();
                        return JsonConvert.DeserializeObject<List<LogEntry>>(json) ?? new List<LogEntry>();
                    }
                }
                catch (Exception ex)
                {
                    LogWarning($"Error reading log file: {ex.Message}");
                    return new List<LogEntry>();
                }
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

            lock (_jsonLogLock) // Verrouiller pendant la lecture/écriture du fichier JSON
            {
                List<LogEntry> existingEntries = new List<LogEntry>();

                try
                {
                    if (File.Exists(logFilePath))
                    {
                        using (FileStream fs = new FileStream(logFilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                        using (StreamReader reader = new StreamReader(fs))
                        {
                            string json = reader.ReadToEnd();
                            existingEntries = JsonConvert.DeserializeObject<List<LogEntry>>(json) ?? new List<LogEntry>();
                        }
                    }

                    existingEntries.Add(entry);

                    string updatedJson = JsonConvert.SerializeObject(existingEntries, Formatting.Indented);

                    using (FileStream fs = new FileStream(logFilePath, FileMode.Create, FileAccess.Write, FileShare.None))
                    using (StreamWriter writer = new StreamWriter(fs))
                    {
                        writer.Write(updatedJson);
                        writer.Flush();
                    }
                }
                catch (Exception ex)
                {
                    // Log the error but continue execution
                    Console.WriteLine($"Error writing to JSON log: {ex.Message}");
                }
            }
        }

        private void WriteToXmlLogFile(LogEntry entry)
        {
            string logFilePath = GetXmlLogFilePath(DateTime.Now);

            lock (_xmlLogLock) // Verrouiller pendant la lecture/écriture du fichier XML
            {
                List<LogEntry> existingEntries = new List<LogEntry>();

                try
                {
                    if (File.Exists(logFilePath))
                    {
                        using (FileStream stream = new FileStream(logFilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                        {
                            existingEntries = (List<LogEntry>)_xmlSerializer.Deserialize(stream);
                        }
                    }

                    existingEntries.Add(entry);

                    using (FileStream stream = new FileStream(logFilePath, FileMode.Create, FileAccess.Write, FileShare.None))
                    {
                        _xmlSerializer.Serialize(stream, existingEntries);
                    }
                }
                catch (Exception ex)
                {
                    LogWarning($"Error writing to XML log file: {ex.Message}");
                }
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
                lock (_warningLogLock) // Verrouiller pendant l'écriture du fichier d'avertissement
                {
                    string logFilePath = Path.Combine(_logDirectory, fileName);

                    using (FileStream fs = new FileStream(logFilePath, FileMode.Append, FileAccess.Write, FileShare.None))
                    using (StreamWriter writer = new StreamWriter(fs))
                    {
                        writer.WriteLine(message);
                        writer.Flush();
                    }
                }
            }
            catch
            {
                // La journalisation ne doit pas provoquer d'erreur dans l'application
            }
        }
    }
}
