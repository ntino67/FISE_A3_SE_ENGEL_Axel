using System;
using Spectre.Console;

namespace EasySave_From_ProSoft
{
    using EasySave_From_ProSoft.View;
    using Spectre.Console;
    using System.Linq;

    public static class Program
    {
        public static void Main(string[] args)
        {
            IConsoleView consoleView = new ConsoleView();
            consoleView.SelectLanguage();
            bool result = consoleView.Confirm("Confirmer ?");
        }
    }
}
