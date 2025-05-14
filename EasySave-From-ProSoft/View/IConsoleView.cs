using System;
using System.Collections.Generic;
using System.Text;
using EasySave_From_ProSoft.Model;

namespace EasySave_From_ProSoft.View
{
    public interface IConsoleView
    {
        public void SelectLanguage();
        public void MainMenu();
        public void MainOptions();
        public string SelectJob(List<BackupJob> jobs, string newJobLabel, string backLabel);
        public string AskForJobName();
        public string ShowJobOptions(BackupJob job, Dictionary<string, string> labels);
        public BackupType SelectBackupType(string prompt, string fullLabel, string diffLabel);
        public bool Confirm(string message);
        public void navigate(string key);
        string BrowseFolders(string currentFolderLabel, string validateLabel, string cancelLabel);
        public void SelectMultipleJobs();

    }
}
