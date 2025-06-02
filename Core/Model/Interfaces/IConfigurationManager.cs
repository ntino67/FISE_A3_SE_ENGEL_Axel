using System.Collections.Generic;

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

        List<string> GetEncryptionFileExtensions();
        void SaveEncryptionFileExtensions(List<string> extensions);
        string GetEncryptionWildcard();
        void SaveEncryptionWildcard(string wildcard);
        List<string> GetPriorityFileExtensions();
        long GetMaxFileSizeKB();
        void SaveMaxFileSizeKB(long maxFileSizeKB);
        void SavePriorityFileExtensions(List<string> extensions);

    }
}
