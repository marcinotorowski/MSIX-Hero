using System.Globalization;
using System.Windows.Data;

namespace Otor.MsixHero.App.Mvvm
{
    public class CultureAwareBinding : Binding
    {
        public CultureAwareBinding()
        {
            this.ConverterCulture = CultureInfo.CurrentUICulture;
        }
    }
}
