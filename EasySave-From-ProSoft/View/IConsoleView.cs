using System;
using System.Collections.Generic;
using System.Text;
using EasySave_From_ProSoft.Model;
using EasySave_From_ProSoft.Utils;

namespace EasySave_From_ProSoft.View
{
    public interface IConsoleView
    {
        public void SelectLanguage();
        public string SelectJob(List<BackupJob> jobs, string newJobLabel, string backLabel);
        public string AskForJobName();
        public string ShowJobOptions(BackupJob job, Dictionary<string, string> labels);
        public BackupType SelectBackupType(string prompt, string fullLabel, string diffLabel);
        public bool Confirm(string message, string yesLabel, string noLabel);
        public void ShowMessage(string message);
        public void ShowError(string message);
        string BrowseFolders(string currentFolderLabel, string validateLabel, string cancelLabel);
        public List<string> SelectMultipleJobs(List<BackupJob> jobs);
        public void ShowLogPaths(string logDirectory, string stateFilePath);
    }
}
