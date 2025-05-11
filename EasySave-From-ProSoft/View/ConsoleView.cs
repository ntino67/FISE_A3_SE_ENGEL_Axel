using EasySave_From_ProSoft.Utils;
using EasySave_From_ProSoft.ViewModel;
using Spectre.Console;
using System;
using System.Collections.Generic;
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
                    .Title($"{LangHelper.GetString("JobOptions")}")
                    .PageSize(10)
                    .AddChoices(jobOptionsChoices.Keys));

            // Get the selected value from the dictionary
            string selectedValue = jobOptionsChoices[jobOptionsSelected];

            // Display the selected value
            AnsiConsole.MarkupLine($"{LangHelper.GetString("SelectedOption")} [green]{selectedValue}[/]");

            // Traiter l'option sélectionnée
            switch (selectedValue)
            {
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
                case "BackToMainMenu":
                    // Retourner au menu principal
                    break;
            }
        }

        public string MainMenu()
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
                    .AddChoices(mainMenuChoices.Keys));

            // Get the selected value from the dictionary
            string selectedValue = mainMenuChoices[MainMenuSelected];

            // Display the selected value
            AnsiConsole.MarkupLine($"{LangHelper.GetString("SelectedOption")} [green]{selectedValue}[/]");

            return selectedValue;
        }

        // Implémentation de la méthode SelectJob()
        public void SelectJob()
        {
            // Obtenir tous les jobs disponibles
            var jobs = ViewModelLocator.GetJobViewModel().GetAllJobs();

            List<string> choices = new List<string>();

            // Ajouter les jobs existants
            foreach (var job in jobs)
            {
                choices.Add(job.Name);
            }

            // Ajouter les options supplémentaires
            choices.Add(LangHelper.GetString("CreateNewJob"));
            choices.Add(LangHelper.GetString("BackToMainMenu"));

            // Afficher le menu de sélection
            string selected = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title(LangHelper.GetString("SelectJobPrompt"))
                    .PageSize(10)
                    .AddChoices(choices));

            if (selected == LangHelper.GetString("CreateNewJob"))
            {
                // Vérifier si le nombre maximum de jobs est atteint
                if (jobs.Count >= 5)
                {
                    AnsiConsole.MarkupLine($"[red]{LangHelper.GetString("MaxJobsReached")}[/]");
                    return;
                }

                // Demander le nom du nouveau job
                string jobName = AnsiConsole.Ask<string>(LangHelper.GetString("EnterJobName"));

                try
                {
                    // Créer le nouveau job
                    ViewModelLocator.GetJobViewModel().CreateNewJob(jobName);
                    AnsiConsole.MarkupLine($"[green]{LangHelper.GetString("JobCreated")}[/]");

                    // Afficher les options du job
                    JobOptions();
                }
                catch (Exception ex)
                {
                    AnsiConsole.MarkupLine($"[red]{ex.Message}[/]");
                }
            }
            else if (selected == LangHelper.GetString("BackToMainMenu"))
            {
                // Ne rien faire, retourner au menu principal
            }
            else
            {
                // Sélectionner le job existant
                var job = jobs.FirstOrDefault(j => j.Name == selected);
                if (job != null)
                {
                    ViewModelLocator.GetJobViewModel().SetCurrentJob(job);
                    JobOptions();
                }
            }
        }

        // Implémentation de la méthode MainOptions()
        public void MainOptions()
        {
            Dictionary<string, string> optionsChoices = new Dictionary<string, string>
            {
                { LangHelper.GetString("ChangeLanguage"), "ChangeLanguage" },
                { LangHelper.GetString("ShowLogs"), "ShowLogs" },
                { LangHelper.GetString("BackToMainMenu"), "BackToMainMenu" }
            };

            string optionSelected = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title(LangHelper.GetString("Options"))
                    .PageSize(10)
                    .AddChoices(optionsChoices.Keys));

            string selectedValue = optionsChoices[optionSelected];

            AnsiConsole.MarkupLine($"{LangHelper.GetString("SelectedOption")} [green]{selectedValue}[/]");

            switch (selectedValue)
            {
                case "ChangeLanguage":
                    SelectLanguage();
                    break;
                case "ShowLogs":
                    ShowLogs();
                    break;
                case "BackToMainMenu":
                    // Ne rien faire, retourner au menu principal
                    break;
            }
        }

        // Méthode pour afficher les logs du jour
        private void ShowLogs()
        {
            var logs = ViewModelLocator.GetLogger().GetTodayLogs();

            if (logs.Count == 0)
            {
                AnsiConsole.MarkupLine($"[yellow]{LangHelper.GetString("NoLogsAvailable")}[/]");
                return;
            }

            var table = new Table();

            table.AddColumn(LangHelper.GetString("Timestamp"));
            table.AddColumn(LangHelper.GetString("JobName"));
            table.AddColumn(LangHelper.GetString("SourcePath"));
            table.AddColumn(LangHelper.GetString("DestinationPath"));
            table.AddColumn(LangHelper.GetString("FileSize"));
            table.AddColumn(LangHelper.GetString("TransferTime"));
            table.AddColumn(LangHelper.GetString("Status"));

            foreach (var log in logs)
            {
                table.AddRow(
                    log.Timestamp.ToString("HH:mm:ss"),
                    log.JobName,
                    ShortenPath(log.SourcePath, 20),
                    ShortenPath(log.DestinationPath, 20),
                    FormatFileSize(log.FileSize),
                    $"{log.TransferTimeMs} ms",
                    log.Status
                );
            }

            AnsiConsole.Write(table);
            AnsiConsole.MarkupLine($"\n{LangHelper.GetString("PressEnterToContinue")}");
            Console.ReadLine();
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

            string selectedLanguageCode = Languages[language];
            LangHelper.ChangeLanguage(selectedLanguageCode);

            // Sauvegarder la langue sélectionnée
            ViewModelLocator.GetConfigManager().SaveLanguage(selectedLanguageCode);

            AnsiConsole.MarkupLine($"{LangHelper.GetString("LanguageSelected")} [green]{LangHelper.GetCurrentLanguage()}[/]");
        }

        private void RenameJob()
        {
            try
            {
                string newName = AnsiConsole.Ask<string>(LangHelper.GetString("EnterNewName"));
                ViewModelLocator.GetJobViewModel().UpdateJobName(newName);
                AnsiConsole.MarkupLine($"[green]{LangHelper.GetString("JobRenamed")}[/]");
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]{ex.Message}[/]");
            }
            JobOptions(); // Retourner au menu des options du job
        }

        private void DefineSourcePath()
        {
            try
            {
                string sourcePath = AnsiConsole.Ask<string>(LangHelper.GetString("EnterSourcePath"));
                ViewModelLocator.GetJobViewModel().UpdateSourcePath(sourcePath);
                AnsiConsole.MarkupLine($"[green]{LangHelper.GetString("SourcePathUpdated")}[/]");
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]{ex.Message}[/]");
            }
            JobOptions(); // Retourner au menu des options du job
        }

        private void DefineTargetPath()
        {
            try
            {
                string targetPath = AnsiConsole.Ask<string>(LangHelper.GetString("EnterTargetPath"));
                ViewModelLocator.GetJobViewModel().UpdateTargetPath(targetPath);
                AnsiConsole.MarkupLine($"[green]{LangHelper.GetString("TargetPathUpdated")}[/]");
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]{ex.Message}[/]");
            }
            JobOptions(); // Retourner au menu des options du job
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
            JobOptions(); // Retourner au menu des options du job
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

            AnsiConsole.MarkupLine($"\n{LangHelper.GetString("PressEnterToContinue")}");
            Console.ReadLine();

            JobOptions(); // Retourner au menu des options du job
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
            JobOptions(); // Retourner au menu des options du job
        }
    }
}
