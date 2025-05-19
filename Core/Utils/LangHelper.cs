using System.Resources;
using System.Reflection;
using System.Globalization;

namespace Core.Utils
{
    public class LangHelper
    {
        private static ResourceManager _rm;
        //static constructor
        static LangHelper()
        {
            _rm = new ResourceManager("Core.Utils.Language.Lang", Assembly.GetExecutingAssembly());
            CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("en-US");
            CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("en-US");
        }

        public static string GetString(string name)
        {
            return _rm.GetString(name);
        }

        public static string GetCurrentLanguage()
        {
            return CultureInfo.CurrentCulture.Name;
        }

        public static void ChangeLanguage(string language)
        {
            CultureInfo culture = new CultureInfo(language);

            CultureInfo.CurrentCulture = culture;
            CultureInfo.CurrentUICulture = culture;
        }

    }
}