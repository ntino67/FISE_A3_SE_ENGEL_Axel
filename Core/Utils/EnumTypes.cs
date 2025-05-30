using System;

namespace Core.Utils
{
    public enum BackupType
    {
        Full,
        Differential,
        Incremental
    }

    public enum Instruction
    {
        Backup,
        Encrypt,
        Decrypt
    }

    public enum JobStatus
    {
        Ready,
        Running,
        Paused,
        Completed,
        Failed,
        Stopped,
        Canceled
    }
}