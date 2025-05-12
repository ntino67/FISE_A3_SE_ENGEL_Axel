using System;
using System.Collections.Generic;
using System.Text;

namespace EasySave_From_ProSoft.View
{
    internal interface IConsoleView
    {
        public void SelectLanguage();
        public void MainMenu();
        public void MainOptions();
        public void SelectJob();
        public void JobOptions();
        public bool Confirm(string message);
        public void navigate(string key);

    }
}
