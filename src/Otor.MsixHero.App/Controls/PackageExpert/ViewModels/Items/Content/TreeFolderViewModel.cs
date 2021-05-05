// MSIX Hero
// Copyright (C) 2021 Marcin Otorowski
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// Full notice:
// https://github.com/marcinotorowski/msix-hero/blob/develop/LICENSE.md

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;

namespace Otor.MsixHero.App.Controls.PackageExpert.ViewModels.Items.Content
{
    public abstract class TreeFolderViewModel : TreeNodeViewModel
    {
        protected readonly TreeViewModel Parent;

        // ReSharper disable once InconsistentNaming
        protected bool isExpanded;

        protected TreeFolderViewModel(TreeViewModel parent)
        {
            this.Parent = parent;
        }
        
        public bool IsExpandable { get; protected set; }

        public bool IsExpanded
        {
            get => this.isExpanded;
            set
            {
                this.SetField(ref this.isExpanded, value);
                if (value)
                {
                    this.HandleExpansion();
                }
            }
        }

        public ObservableCollection<TreeFolderViewModel> Containers { get; } = new ObservableCollection<TreeFolderViewModel>();
        
        public abstract IAsyncEnumerable<TreeFolderViewModel> GetChildren(CancellationToken cancellationToken = default);
        
        public abstract IAsyncEnumerable<TreeNodeViewModel> GetNodes(CancellationToken cancellationToken = default);

        public override string ToString()
        {
            return this.Name;
        }
        
        private async void HandleExpansion()
        {
            this.Containers.Clear();

            await foreach (var subKey in this.GetChildren().ConfigureAwait(true))
            {
                this.Containers.Add(subKey);
            }
        }
    }
}