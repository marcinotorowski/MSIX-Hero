using System.Windows.Media;

namespace Otor.MsixHero.Ui.Modules.Common
{
    public interface IHeaderViewModel
    {
        string Header { get; }

        Geometry Icon { get; }
    }
}