using Otor.MsixHero.App.Modules.PackageManagement.PackageContent.Enums;

namespace Otor.MsixHero.App.Modules.PackageManagement.PackageContent.ViewModel.Common
{
    public interface IPackageContentItem
    {
        public PackageContentViewType Type { get; }

        public bool IsActive { get; set; }
    }
}
