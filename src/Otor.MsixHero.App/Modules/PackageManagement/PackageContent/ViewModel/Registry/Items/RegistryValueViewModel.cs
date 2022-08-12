using Otor.MsixHero.App.Modules.PackageManagement.PackageContent.ViewModel.Common;
using Otor.MsixHero.Appx.Packaging.Registry;

namespace Otor.MsixHero.App.Modules.PackageManagement.PackageContent.ViewModel.Registry.Items
{
    public class RegistryValueViewModel : TreeNodeViewModel
    {
        public RegistryValueViewModel(AppxRegistryValue value)
        {
            this.Name = value.Name;
            this.Path = value.Path;

            if (value.Type != "RegNone")
            {
                Type = value.Type;
                Data = value.Data;
            }
        }

        public string Type { get; }

        public string Data { get; }
    }
}
