using System.Collections.Generic;
using EasySave_From_ProSoft.Model;

namespace EasySave_From_ProSoft.Model.Interfaces
{
    public interface IConfigurationManager
    {
        void SaveJobs(List<BackupJob> jobs);
        List<BackupJob> LoadJobs();
        void SaveLanguage(string languageCode);
        string GetLanguage();
        string GetConfigurationDirectory();
        string GetStateFilePath();
        string GetLogDirectory();
    }
}
