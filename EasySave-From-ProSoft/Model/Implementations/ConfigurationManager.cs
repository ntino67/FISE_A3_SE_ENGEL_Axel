using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using EasySave_From_ProSoft.Model.Interfaces;

namespace EasySave_From_ProSoft.Model.Implementations
{
    public class ConfigurationManager : IConfigurationManager
    {
        private readonly string _configDirectory;
        private readonly string _stateFilePath;
        private readonly string _logDirectory;
        private readonly string _settingsFilePath;

        public ConfigurationManager(string configDirectory)
        {
            _configDirectory = configDirectory;
            _stateFilePath = Path.Combine(configDirectory, "state.json");
            _logDirectory = Path.Combine(configDirectory, "logs");
            _settingsFilePath = Path.Combine(configDirectory, "settings.json");

            Directory.CreateDirectory(configDirectory);
            Directory.CreateDirectory(_logDirectory);
        }

        public void SaveJobs(List<BackupJob> jobs)
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            string json = JsonSerializer.Serialize(jobs, options);
            File.WriteAllText(_stateFilePath, json);
        }

        public List<BackupJob> LoadJobs()
        {
            if (!File.Exists(_stateFilePath))
                return new List<BackupJob>();

            string json = File.ReadAllText(_stateFilePath);
            try
            {
                return JsonSerializer.Deserialize<List<BackupJob>>(json) ?? new List<BackupJob>();
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
                return JsonSerializer.Deserialize<Settings>(json) ?? new Settings();
            }
            catch
            {
                return new Settings();
            }
        }

        private void SaveSettings(Settings settings)
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            string json = JsonSerializer.Serialize(settings, options);
            File.WriteAllText(_settingsFilePath, json);
        }

        private class Settings
        {
            public string Language { get; set; } = "en-US";
        }
    }
}
