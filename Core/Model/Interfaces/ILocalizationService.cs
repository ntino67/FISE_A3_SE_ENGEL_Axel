using System.Collections.Generic;

namespace Core.Model.Interfaces
{
    public interface ILocalizationService
    {
        void ChangeLanguage(string languageCode);
        List<KeyValuePair<string, string>> GetAvailableLanguages();
    }
}
