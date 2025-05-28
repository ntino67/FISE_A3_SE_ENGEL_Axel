using System.Windows;
using Core.Model.Interfaces;

namespace WPF.Services
{
    public class ResourceService : IResourceService
    {
        public string GetString(string key, params object[] formatArgs)
        {
            if (Application.Current.Resources.Contains(key))
            {
                string value = Application.Current.Resources[key] as string;
                if (value != null && formatArgs != null && formatArgs.Length > 0)
                {
                    return string.Format(value, formatArgs);
                }
                return value;
            }

            // Valeur par défaut si la clé n'existe pas
            if (key == "BackupBlockedByApp" && formatArgs != null && formatArgs.Length > 0)
            {
                return string.Format("⚠️ Backup blocked by application: {0}", formatArgs);
            }

            return key;
        }
    }
}
