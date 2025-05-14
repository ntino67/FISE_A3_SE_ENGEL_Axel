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
        public void ShowLogPaths(string logDirectory, string stateFilePath)
        {
            AnsiConsole.MarkupLine($"[grey]{LangHelper.GetString("FileLocation")}[/]");
            AnsiConsole.MarkupLine($"[blue]{LangHelper.GetString("LogPath")}[/]: [yellow]{logDirectory}[/]");
            AnsiConsole.MarkupLine($"[blue]{LangHelper.GetString("StatusPath")}[/]: [yellow]{stateFilePath}[/]");

            AnsiConsole.MarkupLine("\n");
            AnsiConsole.MarkupLine($"[grey]{LangHelper.GetString("PressAKeyToContinue")}[/]");
            Console.ReadKey();
        }
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
                { $"{labels["BackupType"]} (Current: {job.Type})", "BackupType" },
                { labels["Backup"], "Backup" },
                { labels["Reset"], "Reset" },
                { labels["Delete"], "Delete" },
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
                    .Title($"[bold]{LangHelper.GetString("SelectJob")}[/]")
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
            return AnsiConsole.Ask<string>($"[green]{LangHelper.GetString("AskJobName")}[/]");
        }       

        public string BrowseFolders(string currentFolderLabel, string validateLabel, string cancelLabel)
        {
            string currentPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, @"..\..\.."));

            while (true)
            {
                // Only check existence when necessary
                if (!Directory.Exists(currentPath))
                {
                    ShowError("Directory does not exist.");
                    currentPath = Directory.GetParent(currentPath)?.FullName ?? AppContext.BaseDirectory;
                    continue;
                }

                string[] directories;
                try
                {
                    directories = Directory.GetDirectories(currentPath);
                }
                catch
                {
                    ShowError("Cannot access directory.");
                    currentPath = Directory.GetParent(currentPath)?.FullName ?? AppContext.BaseDirectory;
                    continue;
                }

                Dictionary<string, string> choices = new Dictionary<string, string>
                {
                    { "[gray].. Go up one level[/]", ".." },
                    { "[green]Select this folder[/]", "select" },
                    { "[red]Cancel[/]", "cancel" }
                };

                foreach (string dir in directories)
                {
                    string folderName = Path.GetFileName(dir);
                    if (!string.IsNullOrWhiteSpace(folderName))
                    {
                        choices[Markup.Escape(folderName)] = dir;
                    }
                }


                string selection = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title($"[blue]{currentFolderLabel}[/]: [yellow]{currentPath}[/]")
                        .PageSize(15)
                        .AddChoices(choices.Keys)
                );

                switch(choices[selection])
                {
                    case "select":
                        return currentPath;
                    case "cancel":
                        return null;
                    case "..":
                        currentPath = Directory.GetParent(currentPath)?.FullName ?? null;
                        break;
                    default:
                        // Append subfolder to path
                        currentPath = Path.Combine(currentPath, selection);
                        break;
                }

                if (currentPath == null)
                {
                    // User is at a drive root, show all available drives
                    Dictionary<string, string> driveChoices = new Dictionary<string, string>();
                    foreach (DriveInfo drive in DriveInfo.GetDrives().Where(d => d.IsReady))
                    {
                        driveChoices[$"[blue]{drive.Name}[/]"] = drive.RootDirectory.FullName;
                    }
                    driveChoices["[red]Cancel[/]"] = "cancel";

                    string driveSelection = AnsiConsole.Prompt(
                        new SelectionPrompt<string>()
                            .Title("[yellow]Select a drive[/]")
                            .AddChoices(driveChoices.Keys)
                    );

                    if (driveChoices[driveSelection] == "cancel")
                        return null;

                    currentPath = driveChoices[driveSelection];
                    continue;
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

        public List<string> SelectMultipleJobs(List<BackupJob> jobs)
        {
            return AnsiConsole.Prompt(
                new MultiSelectionPrompt<string>()
                    .Title("[bold]Select jobs to run[/]")
                    .InstructionsText("[grey](Use space to toggle, enter to run selected jobs)[/]")
                    .PageSize(10)
                    .AddChoices(jobs.Select(j => j.Name))
            );
        }
    }
}
