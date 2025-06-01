using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Core.Model.Interfaces;
using Core.Utils;
using System.Threading;

namespace Core.Model.Implementations
{
    public class ConfigurationManager : IConfigurationManager
    {
        private readonly string _configDirectory;
        private readonly string _stateFilePath;
        private readonly string _logDirectory;
        private readonly string _settingsFilePath;
        private readonly object _stateLock = new object(); // Objet de verrouillage pour state.json
        private readonly object _settingsLock = new object(); // Objet de verrouillage pour settings.json

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
            lock (_stateLock) // Verrouiller l'accès pendant l'écriture
            {
                string json = JsonConvert.SerializeObject(jobs, Formatting.Indented);

                // Utiliser FileStream avec FileShare.None pour s'assurer que personne d'autre n'accède au fichier
                using (FileStream fs = new FileStream(_stateFilePath, FileMode.Create, FileAccess.Write, FileShare.None))
                using (StreamWriter writer = new StreamWriter(fs))
                {
                    writer.Write(json);
                    writer.Flush();
                }
            }
        }

        public List<BackupJob> LoadJobs()
        {
            lock (_stateLock) // Verrouiller l'accès pendant la lecture
            {
                if (!File.Exists(_stateFilePath))
                    return new List<BackupJob>();

                try
                {
                    string json;
                    using (FileStream fs = new FileStream(_stateFilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                    using (StreamReader reader = new StreamReader(fs))
                    {
                        json = reader.ReadToEnd();
                    }

                    var list = JsonConvert.DeserializeObject<List<BackupJob>>(json) ?? new List<BackupJob>();
                    //Set all status to Ready and reset progress
                    foreach (BackupJob job in list)
                    {
                        job.Status = JobStatus.Ready;
                        job.Progress = 0;
                    }
                    return list;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error loading jobs: {ex.Message}");
                    return new List<BackupJob>();
                }
            }
        }

        public void SaveLanguage(string languageCode)
        {
            Settings settings = LoadSettings();
            settings.Language = languageCode;
            SaveSettings(settings);
        }

        public string GetLanguage()
        {
            Settings settings = LoadSettings();
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
            lock (_settingsLock) // Verrouiller l'accès aux paramètres
            {
                if (!File.Exists(_settingsFilePath))
                    return new Settings();

                try
                {
                    string json;
                    using (FileStream fs = new FileStream(_settingsFilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                    using (StreamReader reader = new StreamReader(fs))
                    {
                        json = reader.ReadToEnd();
                    }

                    return JsonConvert.DeserializeObject<Settings>(json) ?? new Settings();
                }
                catch
                {
                    return new Settings();
                }
            }
        }

        private void SaveSettings(Settings settings)
        {
            lock (_settingsLock) // Verrouiller l'accès aux paramètres
            {
                string json = JsonConvert.SerializeObject(settings, Formatting.Indented);

                using (FileStream fs = new FileStream(_settingsFilePath, FileMode.Create, FileAccess.Write, FileShare.None))
                using (StreamWriter writer = new StreamWriter(fs))
                {
                    writer.Write(json);
                    writer.Flush();
                }
            }
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
            Settings settings = LoadSettings();
            return settings.BlockingApplications ?? new List<string>();
        }

        public void SaveBlockingApplications(List<string> applicationNames)
        {
            Settings settings = LoadSettings();
            settings.BlockingApplications = applicationNames ?? new List<string>();
            SaveSettings(settings);
        }
        public List<string> GetEncryptionFileExtensions()
        {
            Settings settings = LoadSettings();
            return settings.EncryptionFileExtensions ?? new List<string> ();
        }

        public void SaveEncryptionFileExtensions(List<string> extensions)
        {
            Settings settings = LoadSettings();
            settings.EncryptionFileExtensions = extensions ?? new List<string>();
            SaveSettings(settings);
        }

        public string GetEncryptionWildcard()
        {
            Settings settings = LoadSettings();
            return settings.EncryptionWildcard ?? "";
        }

        public void SaveEncryptionWildcard(string wildcard)
        {
            Settings settings = LoadSettings();
            settings.EncryptionWildcard = wildcard;
            SaveSettings(settings);
        }

        public List<string> GetPriorityFileExtensions()
        {
            Settings settings = LoadSettings();
            return settings.PriorityFileExtensions ?? new List<string>();
        }

        public void SavePriorityFileExtensions(List<string> extensions)
        {
            Settings settings = LoadSettings();
            settings.PriorityFileExtensions = extensions ?? new List<string>();
            SaveSettings(settings);
        }

        public long GetMaxFileSizeKB()
        {
            Settings settings = LoadSettings();
            return settings.MaxFileSizeKB;
        }

        public void SaveMaxFileSizeKB(long maxFileSizeKB)
        {
            Settings settings = LoadSettings();
            settings.MaxFileSizeKB = maxFileSizeKB;
            SaveSettings(settings);
        }


    }
}
