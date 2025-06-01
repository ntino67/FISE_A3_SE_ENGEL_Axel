using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Core.Model.Interfaces;
using Core.Utils;

namespace Core.Model.Implementations
{
    public class ConfigurationManager : IConfigurationManager
    {
        private readonly string _configDirectory;
        private readonly string _stateFilePath;
        private readonly string _logDirectory;
        private readonly string _settingsFilePath;

        public ConfigurationManager(string configDirectory)
        {
            // Use the base directory of the application if no config directory is provided
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;

            _configDirectory = baseDirectory;
            _stateFilePath = Path.Combine(baseDirectory, "state.json");

            // Store logs in a subdirectory named "logs" within the config directory
            _logDirectory = Path.Combine(baseDirectory, "logs");
            _settingsFilePath = Path.Combine(baseDirectory, "settings.json");

            Directory.CreateDirectory(_logDirectory);
        }

        public void SaveJobs(List<BackupJob> jobs)
        {
            
            string json = JsonConvert.SerializeObject(jobs, Formatting.Indented);
            File.WriteAllText(_stateFilePath, json);
        }

        public List<BackupJob> LoadJobs()
        {
            if (!File.Exists(_stateFilePath))
                return new List<BackupJob>();

            string json = File.ReadAllText(_stateFilePath);
            try
            {
                var list = JsonConvert.DeserializeObject<List<BackupJob>>(json) ?? new List<BackupJob>();
                //Set all status to Ready and reset progress
                foreach (var job in list)
                {
                    job.Status = JobStatus.Ready;
                    job.Progress = 0;
                }
                return list;
            }
            catch
            {
                return new List<BackupJob>();
            }
        }

        public void SaveLanguage(string languageCode)
        {
            var settings = LoadSettings();
            settings.Language = languageCode;
            SaveSettings(settings);
        }

        public string GetLanguage()
        {
            var settings = LoadSettings();
            return settings.Language;
        }

        public string GetConfigurationDirectory()
        {
            return _configDirectory;
        }

        public string GetStateFilePath()
        {
            return _stateFilePath;
        }

        public string GetLogDirectory()
        {
            return _logDirectory;
        }

        private Settings LoadSettings()
        {
            if (!File.Exists(_settingsFilePath))
                return new Settings();

            string json = File.ReadAllText(_settingsFilePath);
            try
            {
                return JsonConvert.DeserializeObject<Settings>(json) ?? new Settings();
            }
            catch
            {
                return new Settings();
            }
        }

        private void SaveSettings(Settings settings)
        {
            string json = JsonConvert.SerializeObject(settings, Formatting.Indented);
            File.WriteAllText(_settingsFilePath, json);
        }

        private class Settings
        {
            public string Language { get; set; } = "en-US";
            public List<string> BlockingApplications { get; set; } = new List<string>();

            public List<string> EncryptionFileExtensions { get; set; } = new List<string> ();
            public string EncryptionWildcard { get; set; } = "";
            public List<string> PriorityFileExtensions { get; set; } = new List<string>();
            public long MaxFileSizeKB { get; set; } = 1024; // 1 MB par défaut


        }

        public List<string> GetBlockingApplications()
        {
            var settings = LoadSettings();
            return settings.BlockingApplications ?? new List<string>();
        }

        public void SaveBlockingApplications(List<string> applicationNames)
        {
            var settings = LoadSettings();
            settings.BlockingApplications = applicationNames ?? new List<string>();
            SaveSettings(settings);
        }
        public List<string> GetEncryptionFileExtensions()
        {
            var settings = LoadSettings();
            return settings.EncryptionFileExtensions ?? new List<string> ();
        }

        public void SaveEncryptionFileExtensions(List<string> extensions)
        {
            var settings = LoadSettings();
            settings.EncryptionFileExtensions = extensions ?? new List<string>();
            SaveSettings(settings);
        }

        public string GetEncryptionWildcard()
        {
            var settings = LoadSettings();
            return settings.EncryptionWildcard ?? "";
        }

        public void SaveEncryptionWildcard(string wildcard)
        {
            var settings = LoadSettings();
            settings.EncryptionWildcard = wildcard;
            SaveSettings(settings);
        }

        public List<string> GetPriorityFileExtensions()
        {
            var settings = LoadSettings();
            return settings.PriorityFileExtensions ?? new List<string>();
        }

        public void SavePriorityFileExtensions(List<string> extensions)
        {
            var settings = LoadSettings();
            settings.PriorityFileExtensions = extensions ?? new List<string>();
            SaveSettings(settings);
        }

        public long GetMaxFileSizeKB()
        {
            var settings = LoadSettings();
            return settings.MaxFileSizeKB;
        }

        public void SaveMaxFileSizeKB(long maxFileSizeKB)
        {
            var settings = LoadSettings();
            settings.MaxFileSizeKB = maxFileSizeKB;
            SaveSettings(settings);
        }


    }
}
