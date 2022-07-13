// MSIX Hero
// Copyright (C) 2022 Marcin Otorowski
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
using System.Threading.Tasks;
using Otor.MsixHero.App.Helpers;
using Otor.MsixHero.App.Mvvm;
using Otor.MsixHero.Appx.Packaging.Manifest.FileReaders;

namespace Otor.MsixHero.App.Modules.PackageManagement.PackageContent.ViewModel.Common
{
    public abstract class TreeViewModel : NotifyPropertyChanged
    {
        protected readonly string PackageFile;

        protected TreeViewModel(string packageFile)
        {
            PackageFile = packageFile;
        }

        public abstract bool IsAvailable { get; }

        public IAppxFileReader GetAppxReader()
        {
            var reader = FileReaderFactory.CreateFileReader(PackageFile);
            return reader;
        }
    }

    public abstract class TreeViewModel<TContainer, TNode> : TreeViewModel where TContainer : TreeFolderViewModel where TNode : TreeNodeViewModel
    {
        private TContainer selectedContainer;
        private TNode selectedNode;

        protected TreeViewModel(string packageFile) : base(packageFile)
        {
        }

        public TContainer SelectedContainer
        {
            get => selectedContainer;
            set
            {
                SetField(ref selectedContainer, value);

                if (value == null)
                {
                    return;
                }

#pragma warning disable 4014
                SetNodesFromSelectedContainer();
#pragma warning restore 4014
            }
        }

        public TNode SelectedNode
        {
            get => selectedNode;
            set => SetField(ref selectedNode, value);
        }

        private async Task SetNodesFromSelectedContainer()
        {
            var current = selectedContainer;
            if (current == null)
            {
                return;
            }

            Nodes.Clear();

            await foreach (var node in current.GetNodes())
            {
                Nodes.Add((TNode)node);
            }
        }

        public abstract AsyncProperty<IList<TContainer>> Containers { get; }

        public abstract ObservableCollection<TNode> Nodes { get; }
    }
}