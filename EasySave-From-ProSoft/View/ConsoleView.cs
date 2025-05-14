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
        public void SelectLanguage()
        {
            var languages = new Dictionary<string, string>
            {
                { "English", "en-US" },
                { "Français", "fr-FR" }
            };

            string selected = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[bold]Select your language[/]")
                    .PageSize(5)
                    .AddChoices(languages.Keys)
            );

            LangHelper.ChangeLanguage(languages[selected]); // Still okay here for now
        }

        public bool Confirm(string message, string yesLabel, string noLabel)
        {
            Dictionary<string, bool> choices = new Dictionary<string, bool>
            {
                { yesLabel, true },
                { noLabel, false }
            };

            // Confirmation prompt
            string selected = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title(message)
                    .PageSize(2)
                    .AddChoices(choices.Keys)
            );

            return choices[selected];
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

        public string BrowseFolders(string currentFolderLabel, string validateLabel, string cancelLabel)
        {
            string currentPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, @"..\..\..")); // Go to project root

            while (true)
            {
                if (!Directory.Exists(currentPath))
                {
                    ShowError("Directory does not exist.");
                    return null;
                }

                string[] directories = Directory.GetDirectories(currentPath);
                List<string> choices = new List<string>();
                choices.Add(".. Go up one level");
                choices.Add($"[green]{validateLabel}");
                choices.Add($"[red]{cancelLabel}");

                foreach (string dir in directories)
                {
                    choices.Add(Path.GetFileName(dir));
                }

                string selection = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title($"[blue]{currentFolderLabel}[/]: [yellow]{currentPath}[/]")
                        .PageSize(15)
                        .AddChoices(choices)
                );

                if (selection == null)
                    continue;

                if (selection == ".. Go up one level")
                {
                    currentPath = Directory.GetParent(currentPath)?.FullName ?? currentPath;
                }
                else if (selection.StartsWith(validateLabel))
                {
                    return currentPath;
                }
                else if (selection.StartsWith(cancelLabel))
                {
                    return null;
                }
                else
                {
                    currentPath = Path.Combine(currentPath, selection);
                }
            }
        }

        public BackupType SelectBackupType(string prompt, string fullLabel, string diffLabel)
        {
            var choices = new Dictionary<string, BackupType>
            {
                { fullLabel, BackupType.Full },
                { diffLabel, BackupType.Differential },
            };

            string selected = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title($"[bold]{prompt}[/]")
                    .PageSize(10)
                    .AddChoices(choices.Keys)
            );

            return choices[selected];
        }

        public void ShowMessage(string message)
        {
            AnsiConsole.MarkupLine(message);
        }

        public void ShowError(string message)
        {
            AnsiConsole.MarkupLine($"[red]{message}[/]");
        }

        private string ShortenPath(string path, int maxLength)
        {
            if (string.IsNullOrEmpty(path) || path.Length <= maxLength)
                return path;

            int start = maxLength / 2 - 2;
            int end = maxLength / 2 - 1;

            return path.Substring(0, start) + "..." + path.Substring(path.Length - end);
        }

        public List<string> SelectMultipleJobs(List<BackupJob> jobs, string prompt, string instructions, string backLabel)
        {
            var jobNames = jobs.Select(j => j.Name).ToList();
            jobNames.Add(backLabel);

            var selected = AnsiConsole.Prompt(
                new MultiSelectionPrompt<string>()
                    .Title($"[bold]{prompt}[/]")
                    .InstructionsText($"[grey]{instructions}[/]")
                    .PageSize(10)
                    .AddChoices(jobNames)
            );

            return selected;
        }
    }
}
