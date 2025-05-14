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
            _view.ShowMessage($"[green]Job Selected: {_vm.CurrentJob.Name}[/]");

            // This will later call _view.ShowJobOptions(...) and respond to menu options
        }
    }
}
