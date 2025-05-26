using System.Collections.Generic;
using Core.Model;

namespace Core.Model.Interfaces
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

        List<string> GetBlockingApplications();

        void SaveBlockingApplications(List<string> applicationNames);
    }
}
