using System;
using System.Collections.Generic;
using EasySave_From_ProSoft.Model;

namespace EasySave_From_ProSoft.Model.Interfaces
{
    public interface ILogger
    {
        void LogBackupOperation(string jobName, string sourcePath, string destinationPath, long fileSize, long transferTimeMs, string status);
        void LogBackupStart(BackupJob job);
        void LogBackupEnd(BackupJob job, bool success, TimeSpan duration);
        List<LogEntry> GetTodayLogs();
        string GetLogFilePath(DateTime date);
    }
}
