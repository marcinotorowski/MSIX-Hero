using System.Globalization;

namespace Otor.MsixHero.App.Modules.Dialogs.Settings.ViewModel
{
    public class LanguageViewModel
    {
        public LanguageViewModel(string displayName, string languageCode)
        {
            this.DisplayName = displayName;
            this.LanguageCode = languageCode;
        }

        public string DisplayName { get; init; }

        public string LanguageCode { get; init; }

        public static LanguageViewModel FromCultureInfo(CultureInfo ci)
        {
            return new LanguageViewModel(ci.NativeName, ci.Name);
        }

        public static LanguageViewModel FromAuto()
        {
            return new LanguageViewModel(Resources.Localization.Dialogs_Settings_Language_Auto, null);
        }
    }
}
