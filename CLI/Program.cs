namespace EasySave_From_ProSoft
{
    using CLI.View;
    using CLI.ViewModel;
    using Core.ViewModel;

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