using EasySave_From_ProSoft.Utils;
using EasySave_From_ProSoft.ViewModel;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EasySave_From_ProSoft.Model;

namespace EasySave_From_ProSoft.View
{
    internal class ConsoleView : IConsoleView
    {

        public bool Confirm(string message)
        {
            Dictionary<string, bool> keyValuePairs = new Dictionary<string, bool>
            {
                { LangHelper.GetString("Yes"), true },
                { LangHelper.GetString("No"), false }
            };

            // Confirmation prompt
            string confirmationSelected = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title($"{message}")
                    .PageSize(10)
                    .AddChoices(new[] {
                        LangHelper.GetString("Yes"),
                        LangHelper.GetString("No")
                    }));

            // Get the selected value from the dictionary
            bool selectedValue = keyValuePairs.First(kvp => kvp.Key == confirmationSelected).Value;

            return selectedValue;
        }

        public string InputString(string message)
        {
            string input = AnsiConsole.Ask<string>($"{message}");
            return input;
        }

        public string ShowJobOptions(BackupJob job, Dictionary<string, string> labels)
        {
            var choices = new Dictionary<string, string>
            {
                { $"{labels["Rename"]} (Current: {job.Name})", "Rename" },
                { $"{labels["Source"]} (Current: {ShortenPath(job.SourceDirectory, 40)})", "Source" },
                { $"{labels["Target"]} (Current: {ShortenPath(job.TargetDirectory, 40)})", "Target" },
                { labels["BackupType"], "BackupType" },
                { labels["Backup"], "Backup" },
                { labels["Reset"], "Reset" },
                { labels["Back"], "Back" }
            };

            string selected = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                .Title("[bold]Job options[/]")
                .PageSize(10)
                .AddChoices(choices.Keys)
            );

            return choices[selected];
        }

        public void MainMenu()
        {

            Dictionary<string, string> mainMenuChoices = new Dictionary<string, string>
            {
                { LangHelper.GetString("SelectJob"), "SelectJob" },
                { LangHelper.GetString("SelectMultipleJobs"), "SelectMultipleJobs" },
                { LangHelper.GetString("Options"), "Options" },
                { LangHelper.GetString("Exit"), "Exit" }
            };

            // Main menu prompt
            string MainMenuSelected = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title($"[bold]{LangHelper.GetString("MainMenu")}[/]")
                    .PageSize(10)
                    .AddChoices(mainMenuChoices.Keys));
            // Get the selected value from the dictionary
            string selectedValue = mainMenuChoices[MainMenuSelected];
            // Display the selected value
            navigate(selectedValue);
        }

        public void MainOptions()
        {
            Dictionary<string, string> optionsChoices = new Dictionary<string, string>
            {
                { LangHelper.GetString("Language"), "Language" },
                { LangHelper.GetString("LogPath"), "LogPath" },
                { LangHelper.GetString("StatusPath"), "StatusPath" },
                { LangHelper.GetString("BackToMainMenu"), "BackToMainMenu" }
            };

            // Main menu prompt
            string optionSelected = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title($"[bold] {LangHelper.GetString("OptionsMenu")} [/]")
                    .PageSize(10)
                    .AddChoices(optionsChoices.Keys));

            string selectedValue = optionsChoices[optionSelected];

            // Display the selected value
            navigate(selectedValue);
        }

        // Implémentation de la méthode SelectJob()
        public string SelectJob(List<BackupJob> jobs, string newJobLabel, string backLabel)
        {
            List<string> choices = jobs.Select(job => job.Name).ToList();

            choices.Add(newJobLabel);
            choices.Add(backLabel);
            
            string selected = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[bold]Select a job[/]")
                    .PageSize(10)
                    .AddChoices(choices)
            );
            
            if (selected == newJobLabel)
                return "New";

            if (selected == backLabel)
                return "Back";

            return selected;
        }

        public string AskForJobName()
        {
            return AnsiConsole.Ask<string>("[green]Please enter the name of the new backup job:[/]");
        }       

        public void SelectLanguage()
        {
            Dictionary<string, string> Languages = new Dictionary<string, string>
            {
                { "English", "en-US" },
                { "Français", "fr-FR" }
            };

            // Ask for the user language
            string language = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title($"[bold]{LangHelper.GetString("SelectLanguage")}[/]")
                        .PageSize(10)
                        .AddChoices(Languages.Keys));

            string selectedLanguageCode = Languages[language];
            LangHelper.ChangeLanguage(selectedLanguageCode);

            navigate("Options");
        }

        public string BrowseFolders()
        {
            string currentPath = Directory.GetCurrentDirectory();

            while (true)
            {
                string[] directories = Directory.GetDirectories(currentPath);
                List<string> choices = new List<string>
            {
                ".. (Go backward)"
            };

                foreach (string dir in directories)
                {
                    choices.Add(Path.GetFileName(dir));
                }

                choices.Add("[green]Validate this folder[/]");
                choices.Add("[red]Cancel[/]");

                string selection = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title($"[blue]{LangHelper.GetString("CurrentFolder")}[/] : [yellow]{currentPath}[/]")
                        .PageSize(15)
                        .AddChoices(choices)
                );

                switch (selection)
                {
                    case ".. (Go backward)":
                        currentPath = Directory.GetParent(currentPath)?.FullName ?? currentPath;
                        break;

                    case "[green]Validate this folder[/]":
                        return currentPath;

                    case "[red]Cancel[/]":
                        return null;

                    default:
                        currentPath = Path.Combine(currentPath, selection);
                        break;
                }
            }
        }

        // Méthode utilitaire pour raccourcir les chemins trop longs
        private string ShortenPath(string path, int maxLength)
        {
            if (string.IsNullOrEmpty(path))
                return string.Empty;

            if (path.Length <= maxLength)
                return path;

            int startLength = maxLength / 2 - 2;
            int endLength = maxLength / 2 - 1;

            return path.Substring(0, startLength) + "..." + path.Substring(path.Length - endLength);
        }

        // Méthode utilitaire pour formater la taille des fichiers
        private string FormatFileSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            double len = bytes;
            int order = 0;

            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }

            return $"{len:0.##} {sizes[order]}";
        }


        private void RenameJob()
        {
            try
            {
                string newName = InputString(LangHelper.GetString("EnterNewName"));
                ViewModelLocator.GetJobViewModel().UpdateJobName(newName);
                AnsiConsole.MarkupLine($"[green]{LangHelper.GetString("JobRenamed")}[/]");
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]{ex.Message}[/]");
            }
            navigate("JobOptions"); // Retourner au menu des options du job
        }

        private void DefineSourcePath()
        {
            try
            {
                string sourcePath = BrowseFolders();
                ViewModelLocator.GetJobViewModel().UpdateSourcePath(sourcePath);
                AnsiConsole.MarkupLine($"[green]{LangHelper.GetString("SourcePathUpdated")}[/]");
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]{ex.Message}[/]");
            }
            navigate("JobOptions"); // Retourner au menu des options du job
        }

        private void DefineTargetPath()
        {
            try
            {
                string targetPath = BrowseFolders();
                ViewModelLocator.GetJobViewModel().UpdateTargetPath(targetPath);
                AnsiConsole.MarkupLine($"[green]{LangHelper.GetString("TargetPathUpdated")}[/]");
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]{ex.Message}[/]");
            }
            navigate("JobOptions"); // Retourner au menu des options du job
        }

        private void DefineSaveMode()
        {
            Dictionary<string, BackupType> saveModeChoices = new Dictionary<string, BackupType>
            {
                { LangHelper.GetString("FullBackup"), BackupType.Full },
                { LangHelper.GetString("DifferentialBackup"), BackupType.Differential }
            };

            string selected = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title(LangHelper.GetString("SelectBackupType"))
                    .PageSize(10)
                    .AddChoices(saveModeChoices.Keys));

            try
            {
                BackupType selectedType = saveModeChoices[selected];
                ViewModelLocator.GetJobViewModel().UpdateBackupType(selectedType);
                AnsiConsole.MarkupLine($"[green]{LangHelper.GetString("BackupTypeUpdated")}[/]");
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]{ex.Message}[/]");
            }
            navigate("JobOptions"); // Retourner au menu des options du job
        }

        private async Task CreateBackup()
        {
            try
            {
                AnsiConsole.MarkupLine($"[yellow]{LangHelper.GetString("RunningBackup")}[/]");
                bool result = await ViewModelLocator.GetJobViewModel().ExecuteCurrentJob();

                if (result)
                    AnsiConsole.MarkupLine($"[green]{LangHelper.GetString("BackupCompleted")}[/]");
                else
                    AnsiConsole.MarkupLine($"[red]{LangHelper.GetString("BackupFailed")}[/]");
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]{ex.Message}[/]");
            }

            AnsiConsole.MarkupLine($"\n{LangHelper.GetString("SuccessfullBackup")}");

            JobOptions();
        }

        private void ResetJob()
        {
            try
            {
                if (Confirm(LangHelper.GetString("ConfirmReset")))
                {
                    ViewModelLocator.GetJobViewModel().ResetCurrentJob();
                    AnsiConsole.MarkupLine($"[green]{LangHelper.GetString("JobReset")}[/]");
                }
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]{ex.Message}[/]");
            }
            navigate("JobOptions");
        }
        public void navigate(string key)
        {
            switch (key)
            {
                case "SelectJob":
                    {
                        SelectJob();
                        break;
                    }
                case "JobOptions":
                    {
                        JobOptions();
                        break;
                    }
                case "SelectMultipleJobs":
                    {
                        SelectMultipleJobs();
                        break;
                    }
                case "BackToMainMenu":
                    {
                        MainMenu();
                        break;
                    }
                case "Options":
                    {
                        MainOptions();
                        break;
                    }
                case "Language":
                {
                    SelectLanguage();
                    break;
                }
                case "LogPath":
                    {
                        Console.WriteLine(BrowseFolders());
                        break;
                    }
                case "RenameJob":
                    RenameJob();
                    break;
                case "DefineSourcePath":
                    DefineSourcePath();
                    break;
                case "DefineTargetPath":
                    DefineTargetPath();
                    break;
                case "DefineSaveMode":
                    DefineSaveMode();
                    break;
                case "CreateBackup":
                    CreateBackup().Wait();
                    break;
                case "ResetJob":
                    ResetJob();
                    break;
                case "Exit":
                    {
                        Environment.Exit(0);
                        break;
                    }
                default:
                {
                    AnsiConsole.MarkupLine($"{LangHelper.GetString(key)}");
                    break;
                    }


            }
        }

        public void SelectMultipleJobs()
        {
            Dictionary<string, string> jobOptions = new Dictionary<string, string> // Get registered jobs from ViewModel
            {
                { "MyFirstJob", "Job1" },
                { "MySecondJob", "Job2" },
                { "MyThirdJob", "Job3" },
                { LangHelper.GetString("BackToMainMenuAndDoNothing"), "BackToMainMenu" }
            };

            // Job options prompt
            var jobOptionsSelected = AnsiConsole.Prompt(
            new MultiSelectionPrompt<string>()
                .Title($"[bold]{LangHelper.GetString("WhatJobsList")}[/]")
                .PageSize(10)
                .InstructionsText(
                    LangHelper.GetString("JobsListIndication"))
                .AddChoices(jobOptions.Keys));

            // Si "Retour au menu principal" est sélectionné → retour immédiat
            if (jobOptionsSelected.Contains(LangHelper.GetString("BackToMainMenuAndDoNothing")))
            {
                navigate("BackToMainMenu");
                return;
            }

            // Get the selected value from the dictionary
            Console.WriteLine("Runned jobs:");
            foreach (var job in jobOptionsSelected)
            {
                Console.WriteLine(job);
            }
            
        }
    }
}
