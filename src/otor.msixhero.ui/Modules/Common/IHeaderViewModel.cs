using System.Windows.Media;

namespace otor.msixhero.ui.Modules.Common
{
    public interface IHeaderViewModel
    {
        string Header { get; }

        Geometry Icon { get; }
    }
}