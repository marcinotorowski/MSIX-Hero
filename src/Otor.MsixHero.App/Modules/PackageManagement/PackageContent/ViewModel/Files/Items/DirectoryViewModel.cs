﻿using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using Otor.MsixHero.App.Modules.PackageManagement.PackageContent.ViewModel.Common;

namespace Otor.MsixHero.App.Modules.PackageManagement.PackageContent.ViewModel.Files.Items
{
    public class DirectoryViewModel : TreeFolderViewModel
    {
        public DirectoryViewModel(TreeViewModel parent, string fullPath, bool hasChildren) : base(parent)
        {
            this.Path = fullPath;
            this.Name = string.IsNullOrEmpty(fullPath) ? Resources.Localization.PackageExpert_Browser_Root : System.IO.Path.GetFileName(fullPath);
            this.IsExpandable = hasChildren;
            if (hasChildren)
            {
                this.Containers.Add(null);
            }
        }
        
        public override async IAsyncEnumerable<TreeFolderViewModel> GetChildren([EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            using var reader = this.Parent.GetAppxReader();
            await foreach (var directory in reader.EnumerateDirectories(this.Path, cancellationToken))
            {
                var hasChildren = await reader.EnumerateDirectories(directory, cancellationToken).AnyAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
                var dir = new DirectoryViewModel((FileTreeViewModel)this.Parent, directory, hasChildren);
                if (hasChildren)
                {
                    dir.Containers.Add(null); // dummy element
                }
                
                yield return dir;
            }
        }

        public override async IAsyncEnumerable<TreeNodeViewModel> GetNodes([EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            using var reader = this.Parent.GetAppxReader();
            await foreach (var file in reader.EnumerateFiles(this.Path, cancellationToken))
            {
                cancellationToken.ThrowIfCancellationRequested();
                yield return new AppxFileViewModel(file);
            }
        }

        public override string ToString()
        {
            return this.Name;
        }
    }
}
