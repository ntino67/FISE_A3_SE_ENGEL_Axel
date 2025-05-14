using System;
using EasySave_From_ProSoft.Controller;
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
            ViewModelLocator.Initialize();
            var view = new ConsoleView();
            var flow = new ConsoleFlow(view, ViewModelLocator.GetJobViewModel());
            flow.Run();
        }
    }
}
