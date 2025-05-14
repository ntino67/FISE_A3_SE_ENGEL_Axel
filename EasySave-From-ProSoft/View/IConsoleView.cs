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
        public void JobOptions();
        public bool Confirm(string message);
        public void navigate(string key);
        public string BrowseFolders();
        public void SelectMultipleJobs();

    }
}
