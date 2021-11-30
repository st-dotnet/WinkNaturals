using WinkNaturals.Utilities;

namespace WinkNaturals.Models
{
    public static class Language
    {
        public static int GetSelectedLanguageID(string language = "")
        {
            if (language == "")
            {
                language = GetSelectedLanguage();
            }

            switch (language)
            {
                case "es":
                case "es-US":
                    return (int)Languages.Spanish;
                case "en":
                case "en-US":
                default:
                    return (int)Languages.English;
            }
        }
        public static string GetSelectedLanguage()
        {
            var defaultLanguage = "en-US";
            return defaultLanguage;
        }
    }
}
