using System;
using System.Collections.Generic;
using System.Text;
using System.Resources;
using System.Reflection;
using System.Globalization;
using Spectre.Console;
using System.Linq;

namespace Core.Utils
{
    internal class LangHelper
    {
        private static ResourceManager _rm;
        //static constructor
        static LangHelper()
        {
            _rm = new ResourceManager("EasySave_From_ProSoft.Utils.Language.Lang", Assembly.GetExecutingAssembly());
            CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("en_US");
            CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("en_US");
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
