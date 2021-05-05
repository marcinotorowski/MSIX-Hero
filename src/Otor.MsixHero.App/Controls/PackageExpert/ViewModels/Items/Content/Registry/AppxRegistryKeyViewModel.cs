using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using Otor.MsixHero.Appx.Packaging.Registry;

namespace Otor.MsixHero.App.Controls.PackageExpert.ViewModels.Items.Content.Registry
{
    public class AppxRegistryKeyViewModel : TreeFolderViewModel
    {
        public AppxRegistryKeyViewModel(AppxRegistryViewModel parent, AppxRegistryKey model) : base(parent)
        {
            this.Path = model.Path;

            if (this.Path.Equals(AppxRegistryRoots.HKLM, StringComparison.OrdinalIgnoreCase))
            {
                this.Name = "HKLM";
            }
            else if (this.Path.Equals(AppxRegistryRoots.HKCU, StringComparison.OrdinalIgnoreCase))
            {
                this.Name = "HKCU";
            }
            else
            {
                this.Name = model.Path.Substring(model.Path.LastIndexOf('\\') + 1);
            }

            this.IsExpandable = model.HasSubKeys;
            
            if (model.HasSubKeys)
            {
                this.Containers.Add(null);
            }
        }
        
        public override async IAsyncEnumerable<TreeFolderViewModel> GetChildren([EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            using var reader = this.Parent.GetAppxReader();
            if (!reader.FileExists("Registry.dat"))
            {
                yield break;
            }

            await using var registry = reader.GetFile("Registry.dat");
            using var appxRegistryReader = new AppxRegistryReader(registry);
            await foreach (var key in appxRegistryReader.EnumerateKeys(this.Path, cancellationToken))
            {
                yield return new AppxRegistryKeyViewModel((AppxRegistryViewModel)this.Parent, key);
            }
        }
        
        public override async IAsyncEnumerable<TreeNodeViewModel> GetNodes([EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            using var reader = this.Parent.GetAppxReader();
            if (!reader.FileExists("Registry.dat"))
            {
                yield break;
            }

            await using var registry = reader.GetFile("Registry.dat");
            using var appxRegistryReader = new AppxRegistryReader(registry);
            
            await foreach (var value in appxRegistryReader.EnumerateValues(this.Path, cancellationToken))
            {
                yield return new AppxRegistryValueViewModel(value);
            }
        }

        public override string ToString()
        {
            return this.Name;
        }
    }
}
