using EasySave_From_ProSoft.View;
using EasySave_From_ProSoft.ViewModel;
using EasySave_From_ProSoft.Utils;
using EasySave_From_ProSoft.Model;
using System;
using System.Linq;

namespace EasySave_From_ProSoft.Controller
{
    public class ConsoleFlow
    {
        private readonly IConsoleView _view;
        private readonly JobViewModel _vm;

        public ConsoleFlow(IConsoleView view, JobViewModel vm)
        {
            _view = view;
            _vm = vm;
        }

        public void Run()
        {
            // Select language
            _view.SelectLanguage();

            while (true)
            {
                string action = _view.ShowMainMenu(); // You'll refactor this too
                switch (action)
                {
                    case "SelectJob":
                        HandleJobSelection();
                        break;
                    case "Exit":
                        return;
                }
            }
        }

        private void HandleJobSelection()
        {
            string selected = _view.SelectJob(
                _vm.Jobs.ToList(),
                LangHelper.GetString("CreateNewJob"),
                LangHelper.GetString("BackToMainMenu")
            );

            if (selected == "Back")
                return;

            if (selected == "New")
            {
                string jobName = _view.AskForJobName();
                try
                {
                    _vm.CreateNewJob(jobName);
                    HandleJobOptions();
                }
                catch (Exception ex)
                {
                    _view.ShowError(ex.Message);
                }
            }
            else
            {
                var job = _vm.Jobs.FirstOrDefault(j => j.Name == selected);
                if (job != null)
                {
                    _vm.SetCurrentJob(job);
                    HandleJobOptions();
                }
            }
        }

        private void HandleJobOptions()
        {
            while (true)
            {
                string action = _view.ShowJobOptions(_vm.CurrentJob);

                switch (action)
                {
                    case "Rename":
                        string newName = _view.AskForJobName();
                        _vm.UpdateJobName(newName);
                        break;

                    case "Source":
                        string source = _view.BrowseFolders();
                        _vm.UpdateSourcePath(source);
                        break;

                    case "Target":
                        string target = _view.BrowseFolders();
                        _vm.UpdateTargetPath(target);
                        break;

                    case "BackupType":
                        var type = _view.SelectBackupType();
                        _vm.UpdateBackupType(type);
                        break;

                    case "Backup":
                        _vm.RunBackupCommand.Execute(null);
                        break;

                    case "Reset":
                        if (_view.Confirm("Are you sure you want to reset this job?"))
                            _vm.ResetCurrentJob();
                        break;

                    case "Back":
                        return;
                }
            }
        }
    }
}
