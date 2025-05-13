using EasySave_From_ProSoft.ViewModel;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

            // Display the selected value
            AnsiConsole.MarkupLine($"{LangHelper.GetString("SelectedOption")} [green]{selectedValue}[/]");
            return selectedValue;
        }

        public void JobOptions()
        {
            Dictionary<string, string> jobOptionsChoices = new Dictionary<string, string>
            {
                { LangHelper.GetString("RenameJob"), "RenameJob" },
                { LangHelper.GetString("DefineSourcePath"), "DefineSourcePath" },
                { LangHelper.GetString("DefineTargetPath"), "DefineTargetPath" },
                { LangHelper.GetString("DefineSaveMode"), "DefineSaveMode" },
                { LangHelper.GetString("CreateBackup"), "CreateBackup" },
                { LangHelper.GetString("ResetJob"), "ResetJob" },
                { LangHelper.GetString("BackToMainMenu"), "BackToMainMenu" }
            };

            // Job options prompt
            string jobOptionsSelected = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title($"[bold]{LangHelper.GetString("JobOptions")}[/]")
                    .PageSize(10)
                    .AddChoices(new[] {
                        LangHelper.GetString("RenameJob"),
                        LangHelper.GetString("DefineSourcePath"),
                        LangHelper.GetString("DefineTargetPath"),
                        LangHelper.GetString("DefineSaveMode"),
                        LangHelper.GetString("CreateBackup"),
                        LangHelper.GetString("ResetJob"),
                        LangHelper.GetString("BackToMainMenu")
                    }));
            // Get the selected value from the dictionary
            string selectedValue = jobOptionsChoices.First(kvp => kvp.Key == jobOptionsSelected).Value;

            // Display the selected value
            navigate(selectedValue);
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
                    .AddChoices(new[] {
                        LangHelper.GetString("SelectJob"),
                        LangHelper.GetString("SelectMultipleJobs"),
                        LangHelper.GetString("Options"),
                        LangHelper.GetString("Exit")
                    }));
            // Get the selected value from the dictionary
            string selectedValue = mainMenuChoices.First(kvp => kvp.Key == MainMenuSelected).Value;

            // Display the selected value
            navigate(selectedValue);
        }

        public void MainOptions()
        {
            Dictionary<string, string> mainMenuOptions = new Dictionary<string, string>
            {
                { LangHelper.GetString("Language"), "Language" },
                { LangHelper.GetString("LogPath"), "LogPath" },
                { LangHelper.GetString("StatusPath"), "StatusPath" },
                { LangHelper.GetString("BackToMainMenu"), "BackToMainMenu" }
            };

            // Main menu prompt
            string MainMenuSelected = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title($"[bold] {LangHelper.GetString("OptionsMenu")} [/]")
                    .PageSize(10)
                    .AddChoices(new[] {
                        LangHelper.GetString("Language"),
                        LangHelper.GetString("LogPath"),
                        LangHelper.GetString("StatusPath"),
                        LangHelper.GetString("BackToMainMenu")
                    }));
            // Get the selected value from the dictionary
            string selectedValue = mainMenuOptions.First(kvp => kvp.Key == MainMenuSelected).Value;

            // Display the selected value
            navigate(selectedValue);
        }

        public void SelectJob()
        {
            Dictionary<string, string> jobOptions = new Dictionary<string, string> // Get registered jobs from ViewModel
            {
                { "MyFirstJob", "Job1" },
                { "MySecondJob", "Job2" },
                { "MyThirdJob", "Job3" },
            };
            jobOptions.Add(LangHelper.GetString("BackToMainMenu"), "BackToMainMenu");

            // Job options prompt
            string jobOptionsSelected = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title($"[bold] {LangHelper.GetString("SelectJob")} [/]")
                    .PageSize(10)
                    .AddChoices(jobOptions.Keys));

            // Get the selected value from the dictionary
            string selectedValue = jobOptions.First(kvp => kvp.Key == jobOptionsSelected).Value;

            // Display the selected value
            navigate(selectedValue);
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
            string selectedLanguageCode = Languages.First(kvp => kvp.Key == language).Value;
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
        public void navigate(string key)
        {
            switch (key)
            {
                case "SelectJob":
                    {
                        SelectJob();
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
