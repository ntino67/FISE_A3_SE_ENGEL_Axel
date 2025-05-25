using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Policy;
using System.Windows;
using Core.Model.Interfaces;

namespace WPF.Services
{
    public class LocalizationService : ILocalizationService
    {
        public void ChangeLanguage(string languageCode)
        {
            try
            {
                // Change la culture de l'application
                CultureInfo culture = new CultureInfo(languageCode);
                CultureInfo.CurrentUICulture = culture;

                string formattedCode = languageCode.Replace("-", "_");

                // Charge le dictionnaire de ressources approprié
                ResourceDictionary dict = new ResourceDictionary();
                dict.Source = new Uri($"pack://application:,,,/WPF;component/Utils/Language/Dictionary_{formattedCode}.xaml");

               

                // Remplace l'ancien dictionnaire par le nouveau
                Application app = Application.Current;
                ResourceDictionary oldDict = app.Resources.MergedDictionaries
                    .FirstOrDefault(d => d.Source != null && d.Source.OriginalString.Contains("Dictionary_"));

                if (oldDict != null)
                    app.Resources.MergedDictionaries.Remove(oldDict);

                app.Resources.MergedDictionaries.Add(dict);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors du changement de langue : {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public List<KeyValuePair<string, string>> GetAvailableLanguages()
        {
            return new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("en_US", "English"),
                new KeyValuePair<string, string>("fr_FR", "Français")
            };
        }
    }
}
