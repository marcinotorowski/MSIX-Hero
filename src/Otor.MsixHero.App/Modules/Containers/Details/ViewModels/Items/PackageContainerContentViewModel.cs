using Otor.MsixHero.Appx.Packaging;

namespace Otor.MsixHero.App.Modules.Containers.Details.ViewModels.Items
{
    public class PackageContainerContentViewModel : ContainerContentViewModel
    {
        public PackageContainerContentViewModel(PackageEntry package) : base(package.PackageFamilyName)
        {
            if (package.ImageContent != null)
            {
                Image = package.ImageContent;
            }
            else if (package.ImagePath != null)
            {
                Image = package.ImagePath;
            }

            Version = package.Version.ToString();
            DisplayName = package.DisplayName;
            DisplayPublisherName = package.DisplayPublisherName;
        }

        public string DisplayName { get; }

        public string DisplayPublisherName { get; }

        public string Version { get; }

        public object Image { get; }
    }
}
