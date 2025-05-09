namespace EasySave.Model.ImplementIModel
{
    public class Logger
    {
        private static Logger _instance { get; set; }
        public static Logger instance { get; set; }
        public string logFilePath { get; set; }
    }
}
