using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using EasySave_From_ProSoft.Model.Interfaces;

namespace EasySave_From_ProSoft.Model.Implementations
{
    public class Logger : ILogger
    {
        private readonly string _logDirectory;

        public Logger(string logDirectory)
        {
            _logDirectory = logDirectory;
            Directory.CreateDirectory(logDirectory);
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

        public List<LogEntry> GetTodayLogs()
        {
            string logFilePath = GetLogFilePath(DateTime.Now);

            if (!File.Exists(logFilePath))
                return new List<LogEntry>();

            string json = File.ReadAllText(logFilePath);
            try
            {
                return JsonSerializer.Deserialize<List<LogEntry>>(json) ?? new List<LogEntry>();
            }
            catch
            {
                return new List<LogEntry>();
            }
        }

        public string GetLogFilePath(DateTime date)
        {
            return Path.Combine(_logDirectory, $"EasySave_Log_{date:yyyyMMdd}.json");
        }

        private void WriteToLogFile(LogEntry entry)
        {
            string logFilePath = GetLogFilePath(DateTime.Now);

            List<LogEntry> existingEntries = new List<LogEntry>();
            if (File.Exists(logFilePath))
            {
                string json = File.ReadAllText(logFilePath);
                try
                {
                    existingEntries = JsonSerializer.Deserialize<List<LogEntry>>(json) ?? new List<LogEntry>();
                }
                catch
                {
                    // En cas d'erreur, on continue avec une liste vide
                }
            }

            existingEntries.Add(entry);

            var options = new JsonSerializerOptions { WriteIndented = true };
            string updatedJson = JsonSerializer.Serialize(existingEntries, options);
            File.WriteAllText(logFilePath, updatedJson);
        }
    }
}
