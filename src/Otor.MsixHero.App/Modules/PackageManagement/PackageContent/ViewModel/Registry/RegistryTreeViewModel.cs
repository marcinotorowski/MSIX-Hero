using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Otor.MsixHero.App.Helpers;
using Otor.MsixHero.App.Modules.PackageManagement.PackageContent.ViewModel.Common;
using Otor.MsixHero.App.Modules.PackageManagement.PackageContent.ViewModel.Registry.Items;
using Otor.MsixHero.Appx.Packaging.Registry;

namespace Otor.MsixHero.App.Modules.PackageManagement.PackageContent.ViewModel.Registry
{
    public class RegistryTreeViewModel : TreeViewModel<AppxRegistryKeyViewModel, AppxRegistryValueViewModel>
    {
        public RegistryTreeViewModel(string packageFile) : base(packageFile)
        {
            Containers = new AsyncProperty<IList<AppxRegistryKeyViewModel>>(GetRootContainers());

            var nodesCollection = new ObservableCollection<AppxRegistryValueViewModel>();
            Nodes = nodesCollection;
        }

        public override bool IsAvailable => GetAppxReader().FileExists("registry.dat");

        public override AsyncProperty<IList<AppxRegistryKeyViewModel>> Containers { get; }

        public override ObservableCollection<AppxRegistryValueViewModel> Nodes { get; }

        public async Task<IList<AppxRegistryKeyViewModel>> GetRootContainers()
        {
            if (!IsAvailable)
            {
                return null;
            }

            var roots = new List<AppxRegistryKeyViewModel>();

            using var appxReader = GetAppxReader();
            await using var registry = appxReader.GetFile("Registry.dat");
            using var reader = new AppxRegistryReader(registry);
            await foreach (var root in reader.EnumerateKeys(AppxRegistryRoots.Root).ConfigureAwait(false))
            {
                roots.Add(new AppxRegistryKeyViewModel(this, root));
            }

            return roots;
        }
    }
}
