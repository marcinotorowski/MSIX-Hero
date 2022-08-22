using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Otor.MsixHero.App.Modules.PackageManagement.PackageContent.ViewModel.Common;
using Otor.MsixHero.App.Mvvm;
using Otor.MsixHero.Appx.Packaging.Registry;

namespace Otor.MsixHero.App.Modules.PackageManagement.PackageContent.ViewModel.Registry.Items
{
    public class RegistryTreeViewModel : TreeViewModel<RegistryKeyViewModel, RegistryValueViewModel>
    {
        public RegistryTreeViewModel(string packageFile) : base(packageFile)
        {
            Containers = new AsyncProperty<IList<RegistryKeyViewModel>>(GetRootContainers());

            var nodesCollection = new ObservableCollection<RegistryValueViewModel>();
            Nodes = nodesCollection;
        }

        public override bool IsAvailable => GetAppxReader().FileExists("registry.dat");

        public override AsyncProperty<IList<RegistryKeyViewModel>> Containers { get; }

        public override ObservableCollection<RegistryValueViewModel> Nodes { get; }

        public async Task<IList<RegistryKeyViewModel>> GetRootContainers()
        {
            if (!IsAvailable)
            {
                return null;
            }

            var roots = new List<RegistryKeyViewModel>();

            using var appxReader = GetAppxReader();
            await using var registry = appxReader.GetFile("Registry.dat");
            using var reader = new AppxRegistryReader(registry);
            await foreach (var root in reader.EnumerateKeys(AppxRegistryRoots.Root).ConfigureAwait(false))
            {
                roots.Add(new RegistryKeyViewModel(this, root));
            }

            return roots;
        }
    }
}
