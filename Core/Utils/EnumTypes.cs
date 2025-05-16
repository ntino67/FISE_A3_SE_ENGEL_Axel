using System;

namespace Core.Utils
{
    public enum BackupType
    {
        Full,
        Differential
    }

    public enum JobStatus
    {
        Ready,
        Running,
        Completed,
        Failed,
        Canceled
    }
}