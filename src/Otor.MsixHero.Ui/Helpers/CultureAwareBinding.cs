using System.Globalization;
using System.Windows.Data;

namespace Otor.MsixHero.Ui.Helpers
{
    public class CultureAwareBinding : Binding
    {
        public CultureAwareBinding()
        {
            this.ConverterCulture = CultureInfo.CurrentUICulture;
        }
    }
}
