using EasySave_From_ProSoft.View;
using EasySave_From_ProSoft.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using Spectre.Console;
using System.Threading.Tasks;

namespace EasySave_From_ProSoft.ViewModel
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
                string action = ShowMainMenu();

                switch (action)
                {
                    case "SelectJob":
                        HandleJobSelection();
                        break;
                    case "SelectMultipleJobs":
                        HandleMultipleJobs().GetAwaiter().GetResult();
                        break;
                    case "Options":
                        HandleGlobalOptions();
                        break;
                    case "Exit":
                        return;
                }
            }
        }

        private string ShowMainMenu()
        {
            var choices = new Dictionary<string, string>
            {
                { LangHelper.GetString("SelectJob"), "SelectJob" },
                { LangHelper.GetString("SelectMultipleJobs"), "SelectMultipleJobs" },
                { LangHelper.GetString("Options"), "Options" },
                { LangHelper.GetString("Exit"), "Exit" }
            };

            string selected = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title($"[bold]{LangHelper.GetString("MainMenu")}[/]")
                    .PageSize(10)
                    .AddChoices(choices.Keys)
            );

            return choices[selected];
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
                    _view.ShowMessage($"[green]{string.Format(LangHelper.GetString("JobCreated"), jobName)}[/]");
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

        private async Task HandleMultipleJobs()
        {
            if (_vm.Jobs.Count == 0)
            {
                _view.ShowMessage("[yellow]No jobs available.[/]");
                return;
            }

            // Confirm before showing the list
            bool proceed = _view.Confirm(
                LangHelper.GetString("WhatJobsList"),
                LangHelper.GetString("Yes"),
                LangHelper.GetString("BackToMainMenuAndDoNothing")
            );

            if (!proceed)
                return;

            // Show multiselect (clean, no back option here)
            var selectedNames = _view.SelectMultipleJobs(_vm.Jobs.ToList());

            foreach (var name in selectedNames)
            {
                var job = _vm.Jobs.FirstOrDefault(j => j.Name == name);
                if (job != null)
                {
                    _vm.SetCurrentJob(job);
                    _view.ShowMessage($"[yellow]Running job: {job.Name}...[/]");
                    var result = await _vm.ExecuteCurrentJob();
                    string resultText = result
                        ? LangHelper.GetString("BackupCompleted")
                        : LangHelper.GetString("BackupFailed");

                    _view.ShowMessage($"[green]{resultText}[/]");
                }
            }
        }

        private void HandleJobOptions()
        {
            var job = _vm.CurrentJob;

            var jobMenuLabels = new Dictionary<string, string>
            {
                { "Rename", LangHelper.GetString("RenameJob") },
                { "Source", LangHelper.GetString("DefineSourcePath") },
                { "Target", LangHelper.GetString("DefineTargetPath") },
                { "BackupType", LangHelper.GetString("DefineSaveMode") },
                { "Backup", LangHelper.GetString("CreateBackup") },
                { "Reset", LangHelper.GetString("ResetJob") },
                { "Delete", LangHelper.GetString("DeleteJob") },
                { "Back", LangHelper.GetString("BackToMainMenu") }
            };

            while (true)
            {
                string action = _view.ShowJobOptions(job, jobMenuLabels);

                switch (action)
                {
                    case "Rename":
                    {
                        string newName = _view.AskForJobName();
                        _vm.UpdateJobName(newName);
                        _view.ShowMessage($"[green]{LangHelper.GetString("JobRenamed")}[/]");
                        break;
                    }

                    case "Source":
                    {
                        string folder = _view.BrowseFolders(
                            LangHelper.GetString("CurrentFolder"),
                            LangHelper.GetString("ValidateFolder"),
                            LangHelper.GetString("Cancel")
                        );
                        if (folder != null)
                        {
                            _vm.UpdateSourcePath(folder);
                            _view.ShowMessage($"[green]{LangHelper.GetString("SourcePathUpdated")}[/]");
                        }
                        break;
                    }

                    case "Target":
                    {
                        string folder = _view.BrowseFolders(
                            LangHelper.GetString("CurrentFolder"),
                            LangHelper.GetString("ValidateFolder"),
                            LangHelper.GetString("Cancel")
                        );
                        if (folder != null)
                        {
                            _vm.UpdateTargetPath(folder);
                            _view.ShowMessage($"[green]{LangHelper.GetString("TargetPathUpdated")}[/]");
                        }
                        break;
                    }

                    case "BackupType":
                    {
                        BackupType selected = _view.SelectBackupType(
                            LangHelper.GetString("SelectBackupType"),
                            LangHelper.GetString("FullBackup"),
                            LangHelper.GetString("DifferentialBackup")
                        );
                        _vm.UpdateBackupType(selected);
                        _view.ShowMessage($"[green]{LangHelper.GetString("BackupTypeUpdated")}[/]");
                        break;
                    }

                    case "Backup":
                    {
                        _view.ShowMessage($"[yellow]{LangHelper.GetString("RunningBackup")}[/]");
                        bool result = _vm.ExecuteCurrentJob().Result;
                        string resultMsg = result
                            ? LangHelper.GetString("BackupCompleted")
                            : LangHelper.GetString("BackupFailed");
                        _view.ShowMessage($"[green]{resultMsg}[/]");
                        break;
                    }

                    case "Reset":
                    {
                        bool confirm = _view.Confirm(
                            LangHelper.GetString("ConfirmReset"),
                            LangHelper.GetString("Yes"),
                            LangHelper.GetString("No")
                        );
                        if (confirm)
                        {
                            _vm.ResetCurrentJob();
                            _view.ShowMessage($"[green]{LangHelper.GetString("JobReset")}[/]");
                        }
                        break;
                    }

                    case "Delete":
                        {
                            bool confirm = _view.Confirm(
                                LangHelper.GetString("ConfirmDelete"),
                                LangHelper.GetString("Yes"),
                                LangHelper.GetString("No")
                            );
                            if (confirm)
                            {
                                string deletedName = job.Name;
                                _vm.DeleteJob(job.Id);
                                _view.ShowMessage($"[green]{string.Format(LangHelper.GetString("JobDeleted"), deletedName)}[/]");
                                return;
                            }
                            break;
                        }

                    case "Back":
                        return;
                }
            }
        }
        
        private void HandleGlobalOptions()
        {
            while (true)
            {
                var options = new Dictionary<string, string>
                {
                    { "Lang", LangHelper.GetString("SelectLanguage") },
                    { "LogPaths", LangHelper.GetString("LogPaths") },
                    { "Back", LangHelper.GetString("BackToMainMenu") }
                };

                string selected = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title($"[bold]{LangHelper.GetString("OptionsMenu")}[/]")
                        .PageSize(5)
                        .AddChoices(options.Values)
                );

                if (selected == options["Lang"])
                {
                    _view.SelectLanguage();
                }
                else if (selected == options["LogPaths"])
                {
                    // Show log paths   
                    var configManager = ViewModelLocator.GetConfigurationManager();
                    _view.ShowLogPaths(configManager.GetLogDirectory(), configManager.GetStateFilePath());

                }
                else if (selected == options["Back"])
                {
                    return;
                }
            }
        }
    }
}
