using Otor.MsixHero.App.Modules.PackageManagement.PackageContent.Enums;

namespace Otor.MsixHero.App.Modules.PackageManagement.PackageContent.ViewModel.Common
{
    public interface IPackageContentItemNavigation
    {
        void SetCurrentItem(PackageContentViewType type);

        IPackageContentItem CurrentItem { get; set; }
    }
}
