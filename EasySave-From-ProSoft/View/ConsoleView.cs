using EasySave_From_ProSoft.ViewModel;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
                    .Title($"{LangHelper.GetString("JobOptions")}")
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
            naviguate(selectedValue);
        }

        public void MainMenu()
        {

            Dictionary<string, string> mainMenuChoices = new Dictionary<string, string>
            {
                { LangHelper.GetString("SelectJob"), "SelectJob" },
                { LangHelper.GetString("RunAllJobs"), "RunAllJobs" },
                { LangHelper.GetString("Options"), "Options" },
                { LangHelper.GetString("Exit"), "Exit" }
            };

            // Main menu prompt
            string MainMenuSelected = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title($"{LangHelper.GetString("MainMenu")}")
                    .PageSize(10)
                    .AddChoices(new[] {
                        LangHelper.GetString("SelectJob"),
                        LangHelper.GetString("RunAllJobs"),
                        LangHelper.GetString("Options"),
                        LangHelper.GetString("Exit")
                    }));
            // Get the selected value from the dictionary
            string selectedValue = mainMenuChoices.First(kvp => kvp.Key == MainMenuSelected).Value;

            // Display the selected value
            naviguate(selectedValue);
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
                    .Title($"{LangHelper.GetString("OptionsMenu")}")
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
            naviguate(selectedValue);
        }

        public void SelectJob()
        {
            throw new NotImplementedException();
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
                        .Title($"{LangHelper.GetString("SelectLanguage")}")
                        .PageSize(10)
                        .AddChoices(Languages.Keys));
            string selectedLanguageCode = Languages.First(kvp => kvp.Key == language).Value;
            LangHelper.ChangeLanguage(selectedLanguageCode);

            naviguate("Options");
        }

        public void naviguate(string key)
        {
            switch (key)
            {
                case "SelectJob":
                    {
                        SelectJob();
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
                default:
                {
                    AnsiConsole.MarkupLine($"{LangHelper.GetString(key)}");
                    break;
                    }


            }
        }
    }
}
