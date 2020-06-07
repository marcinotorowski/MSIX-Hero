using System.Globalization;
using System.Windows.Data;

namespace otor.msixhero.ui.Helpers
{
    public class CultureAwareBinding : Binding
    {
        public CultureAwareBinding()
        {
            this.ConverterCulture = CultureInfo.CurrentUICulture;
        }
    }
}
