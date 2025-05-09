using System;
using Spectre.Console;

namespace EasySave_From_ProSoft
{
    using EasySave_From_ProSoft.View;
    using EasySave_From_ProSoft.ViewModel;
    using Spectre.Console;
    using System.Linq;

    public static class Program
    {
        public static void Main(string[] args)
        {
            IConsoleView consoleView = new ConsoleView();
            AnsiConsole.MarkupLine($"Nothing is implemented yet, go to [green]ConsoleView[/] for showing things to user.");
        }
    }
}
