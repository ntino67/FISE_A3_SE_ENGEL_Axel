using System;
using System.Collections.Generic;
using Core.Model;

namespace Core.Model.Interfaces
{
    public interface ILogger
    {
        void LogBackupOperation(string jobName, string sourcePath, string destinationPath, long fileSize, long transferTimeMs, string status);
        void LogBackupStart(BackupJob job);
        void LogBackupEnd(BackupJob job, bool success, TimeSpan duration);
        List<LogEntry> GetTodayLogs();
        string GetLogFilePath(DateTime date);

        void LogWarning(string message);
        void LogEncryptionStart(BackupJob job);
        void LogEncryptionEnd(BackupJob job,bool success, long duration);
    }
}
