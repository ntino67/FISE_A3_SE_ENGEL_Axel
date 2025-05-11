using System;

namespace EasySave_From_ProSoft.Utils
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
