using otor.msixhero.lib.BusinessLayer.Models.Configuration;

namespace otor.msixhero.lib.BusinessLayer.State
{
    public interface IApplicationState
    {
        IPackageListState Packages { get; }    

        ILocalSettings LocalSettings { get; }

        Configuration Configuration { get; }

        bool IsElevated { get; }

        bool HasSelfElevated { get; }
    }
}