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

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Otor.MsixHero.App.Modules.PackageManagement.PackageContent.ViewModel.Common;
using Otor.MsixHero.App.Mvvm;
using Otor.MsixHero.Appx.Packaging.Manifest.FileReaders;
using Otor.MsixHero.Infrastructure.Helpers;
using Prism.Commands;

namespace Otor.MsixHero.App.Modules.PackageManagement.PackageContent.ViewModel.Files.Items;

public class FileTreeViewModel : TreeViewModel<DirectoryViewModel, AppxFileViewModel>
{
    private readonly IAppxFileViewer _fileViewer;
    private readonly FileInvoker _fileInvoker;

    public FileTreeViewModel(
        string packageFile,
        IAppxFileViewer fileViewer,
        FileInvoker fileInvoker) : base(packageFile)
    {
        _fileViewer = fileViewer;
        _fileInvoker = fileInvoker;
        var rootContainersTask = GetRootContainers();
        var nodesCollection = new ObservableCollection<AppxFileViewModel>();
        Nodes = nodesCollection;
        View = new DelegateCommand(OnView);

        var containers = new AsyncProperty<IList<DirectoryViewModel>>();
        Containers = containers;
#pragma warning disable 4014
        containers.Loaded += OnContainersLoaded;
        containers.Load(rootContainersTask);
#pragma warning restore 4014
    }

    private async void OnView()
    {
        var selectedFile = SelectedNode;
        if (selectedFile == null)
        {
            return;
        }

        using var reader = FileReaderFactory.CreateFileReader(PackageFile);
        var path = await _fileViewer.GetDiskPath(reader, selectedFile.Path).ConfigureAwait(false);

        ExceptionGuard.Guard(() => { _fileInvoker.Execute(path, true); });
    }

    private void OnContainersLoaded(object sender, EventArgs e)
    {
        SelectedContainer = Containers.CurrentValue.FirstOrDefault();
    }

    public override bool IsAvailable => true;

    public override ObservableCollection<AppxFileViewModel> Nodes { get; }

    public override AsyncProperty<IList<DirectoryViewModel>> Containers { get; }

    public async Task<IList<DirectoryViewModel>> GetRootContainers()
    {
        var roots = new List<DirectoryViewModel>();
        using var appxReader = GetAppxReader();
        var hasChildren = await appxReader.EnumerateDirectories().AnyAsync().ConfigureAwait(false);
        roots.Add(new DirectoryViewModel(this, null, hasChildren));
        return roots;
    }

    public ICommand View { get; }
}