using System;
using EasySave_From_ProSoft.View;
using EasySave_From_ProSoft.ViewModel;
using EasySave_From_ProSoft.Utils;
using Spectre.Console;
using System.Linq;
using System.Threading.Tasks;

namespace EasySave_From_ProSoft
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                // Initialiser le ViewModelLocator
                ViewModelLocator.Initialize();

                // Initialiser la vue console
                IConsoleView consoleView = new ConsoleView();

                // Afficher un message de bienvenue
                AnsiConsole.MarkupLine("[green]====== EasySave 1.0 ======[/]");

                // Sélectionner la langue si première utilisation
                string language = ViewModelLocator.GetConfigManager().GetLanguage();
                if (string.IsNullOrEmpty(language))
                {
                    consoleView.SelectLanguage();
                }
                else
                {
                    // Sinon, définir la langue sauvegardée
                    LangHelper.ChangeLanguage(language);
                }

                // Boucle principale
                bool exit = false;
                while (!exit)
                {
                    // Afficher le menu principal
                    string choice = consoleView.MainMenu();

                    switch (choice)
                    {
                        case "SelectJob":
                            consoleView.SelectJob();
                            break;
                        case "RunAllJobs":
                            RunAllJobs(consoleView).Wait();
                            break;
                        case "Options":
                            consoleView.MainOptions();
                            break;
                        case "Exit":
                            exit = true;
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]Une erreur est survenue: {ex.Message}[/]");
                Console.ReadLine();
            }
        }

        // Méthode pour exécuter tous les jobs
        private static async Task RunAllJobs(IConsoleView consoleView)
        {
            AnsiConsole.MarkupLine($"[yellow]{LangHelper.GetString("RunningAllJobs")}[/]");

            var results = await ViewModelLocator.GetJobViewModel().ExecuteAllJobs();

            int successCount = results.Count(r => r);

            AnsiConsole.MarkupLine($"[green]{LangHelper.GetString("JobsCompleted")}: {successCount}/{results.Count}[/]");

            AnsiConsole.MarkupLine($"\n{LangHelper.GetString("PressEnterToContinue")}");
            Console.ReadLine();
        }
    }
}
