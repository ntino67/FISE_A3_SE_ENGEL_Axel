using System;
using System.Collections.Generic;
using System.IO;
using EasySave_From_ProSoft.Model.IModel;
using EasySave.Model.ImplementIModel;

namespace EasySave_From_ProSoft.Model.ImplementIModel
{
    public sealed class Logger : ILogger
    {
        private Logger() { }

        private static Logger _instance { get; set; }
        public string logFilePath { get; set; }

        public static Logger GetInstance()
        {
            if (_instance == null)
            {
                _instance = new Logger();
            }
            return _instance;
        }

        public void Log(LogEntry entry)
        {
            throw new Exception("not implemented yet");
        }
        
        public List<LogEntry> LoadLog(DateTime date)
        {
            throw new Exception("not implemented yet");
        }
    }
}
