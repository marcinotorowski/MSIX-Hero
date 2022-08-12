using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using Otor.MsixHero.App.Modules.PackageManagement.PackageContent.ViewModel.Common;
using Otor.MsixHero.Appx.Packaging.Registry;

namespace Otor.MsixHero.App.Modules.PackageManagement.PackageContent.ViewModel.Registry.Items
{
    public class RegistryKeyViewModel : TreeFolderViewModel
    {
        public RegistryKeyViewModel(TreeViewModel parent, AppxRegistryKey model) : base(parent)
        {
            Path = model.Path;

            if (Path.Equals(AppxRegistryRoots.HKLM, StringComparison.OrdinalIgnoreCase))
            {
                Name = "HKLM";
            }
            else if (Path.Equals(AppxRegistryRoots.HKCU, StringComparison.OrdinalIgnoreCase))
            {
                Name = "HKCU";
            }
            else
            {
                Name = model.Path.Substring(model.Path.LastIndexOf('\\') + 1);
            }

            IsExpandable = model.HasSubKeys;

            if (model.HasSubKeys)
            {
                Containers.Add(null);
            }
        }

        public override async IAsyncEnumerable<TreeFolderViewModel> GetChildren([EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            using var reader = Parent.GetAppxReader();
            if (!reader.FileExists("Registry.dat"))
            {
                yield break;
            }

            await using var registry = reader.GetFile("Registry.dat");
            using var appxRegistryReader = new AppxRegistryReader(registry);
            await foreach (var key in appxRegistryReader.EnumerateKeys(Path, cancellationToken))
            {
                yield return new RegistryKeyViewModel((RegistryTreeViewModel)Parent, key);
            }
        }

        public override async IAsyncEnumerable<TreeNodeViewModel> GetNodes([EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            using var reader = Parent.GetAppxReader();
            if (!reader.FileExists("Registry.dat"))
            {
                yield break;
            }

            await using var registry = reader.GetFile("Registry.dat");
            using var appxRegistryReader = new AppxRegistryReader(registry);

            await foreach (var value in appxRegistryReader.EnumerateValues(Path, cancellationToken))
            {
                yield return new RegistryValueViewModel(value);
            }
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
