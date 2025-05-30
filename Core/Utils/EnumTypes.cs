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
        Paused,
        Stopped,
        Completed,
        Failed,
        Canceled,
        ForcePaused
    }

    public enum Instruction
    {
        Encrypt,
        Decrypt,
        Backup
    }
}