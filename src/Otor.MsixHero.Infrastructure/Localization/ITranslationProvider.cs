using System.Collections.Generic;
using System.Globalization;

namespace Otor.MsixHero.Infrastructure.Localization
{
    public interface ITranslationProvider
    {
        IEnumerable<CultureInfo> GetAvailableTranslations();
    }
}
