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
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Otor.MsixHero.App.Helpers;
using Otor.MsixHero.App.Hero;
using Otor.MsixHero.App.Modules.PackageManagement.PackageContent.Enums;
using Otor.MsixHero.App.Modules.PackageManagement.PackageContent.ViewModel.Common;
using Otor.MsixHero.App.Modules.PackageManagement.PackageContent.ViewModel.Overview.Summaries;
using Otor.MsixHero.App.Mvvm;
using Otor.MsixHero.Appx.Packaging;
using Otor.MsixHero.Appx.Packaging.Installation.Entities;
using Otor.MsixHero.Appx.Packaging.Manifest;
using Otor.MsixHero.Appx.Packaging.Manifest.Entities;
using Otor.MsixHero.Appx.Packaging.Manifest.FileReaders;
using Otor.MsixHero.Elevation;
using Otor.MsixHero.Infrastructure.Services;
using Prism.Events;

namespace Otor.MsixHero.App.Modules.PackageManagement.PackageContent.ViewModel.Overview;

public class PackageOverviewViewModel : NotifyPropertyChanged, ILoadPackage, IPackageContentItem
{
    private readonly IList<ILoadPackage> _loadPackageHandlers = new List<ILoadPackage>();
    private bool _isActive;
    private Tuple<AppxPackage, string> _pendingPackage;

    public PackageOverviewViewModel(
        IPackageContentItemNavigation navigation,
        IInteractionService interactionService,
        IConfigurationService configurationService,
        IEventAggregator eventAggregator,
        IUacElevation uacElevation,
        PrismServices prismServices)
    {
        _loadPackageHandlers.Add(SummarySummaryPackageName = new SummaryPackageNameViewModel(navigation, prismServices));
        _loadPackageHandlers.Add(SummaryPackagingInformation = new SummaryPackagingInformationViewModel(navigation, prismServices));
        _loadPackageHandlers.Add(SummarySummarySignature = new SummarySignatureViewModel(navigation, interactionService, uacElevation));
        _loadPackageHandlers.Add(SummarySummaryDependencies = new SummaryDependenciesViewModel(navigation));
        _loadPackageHandlers.Add(SummarySummaryInstallation = new SummaryInstallationViewModel(navigation, uacElevation));
        _loadPackageHandlers.Add(SummaryFiles = new SummaryFilesViewModel(navigation));
        _loadPackageHandlers.Add(SummaryRegistry = new SummaryRegistryViewModel(navigation));
        _loadPackageHandlers.Add(SummarySummaryCapabilities = new SummaryCapabilitiesViewModel(navigation));
        _loadPackageHandlers.Add(SummaryApplications = new SummaryApplicationsViewModel(navigation));
    }

    public PackageContentViewType Type => PackageContentViewType.Overview;
    
    public bool IsActive
    {
        get => _isActive;
        set
        {
            if (!SetField(ref _isActive, value))
            {
                return;
            }

            if (value && _pendingPackage != null)
            {
                var package = _pendingPackage.Item1;
                var path = _pendingPackage.Item2;

                _pendingPackage = null;
#pragma warning disable CS4014
                LoadPackage(package, path, CancellationToken.None);
#pragma warning restore CS4014
            }
        }
    }

    public SummaryPackageNameViewModel SummarySummaryPackageName { get; }

    public SummaryFilesViewModel SummaryFiles { get; }

    public SummaryApplicationsViewModel SummaryApplications { get; }

    public SummaryRegistryViewModel SummaryRegistry { get; }

    public SummaryPackagingInformationViewModel SummaryPackagingInformation { get; }

    public SummarySignatureViewModel SummarySummarySignature { get; }

    public SummaryDependenciesViewModel SummarySummaryDependencies { get; }

    public SummaryInstallationViewModel SummarySummaryInstallation { get; }

    public SummaryCapabilitiesViewModel SummarySummaryCapabilities { get; }

    public ProgressProperty Progress { get; } = new ProgressProperty();

    public string FilePath { get; private set; }

    public async Task LoadPackage(AppxPackage model, string filePath, CancellationToken cancellationToken)
    {
        if (!IsActive)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _pendingPackage = new Tuple<AppxPackage, string>(model, filePath);
            return;
        }

        var originalProgress = Progress.IsLoading;
        Progress.IsLoading = true;
        try
        {
            await Task.WhenAll(_loadPackageHandlers.Select(t => t.LoadPackage(model, filePath, cancellationToken))).ConfigureAwait(false);
            this.FilePath = filePath;
            OnPropertyChanged(nameof(FilePath));

        }
        finally
        {
            Progress.IsLoading = originalProgress;
        }
    }

    public async Task LoadPackage(IAppxFileReader reader, CancellationToken cancellationToken)
    {
        var originalProgress = Progress.IsLoading;

        this.Progress.IsLoading = true;

        try
        {
            var path = reader switch
            {
                FileInfoFileReaderAdapter file => file.FilePath,
                IAppxDiskFileReader fileReader => Path.Combine(fileReader.RootDirectory, FileConstants.AppxManifestFile),
                ZipArchiveFileReaderAdapter zipReader => zipReader.PackagePath,
                _ => throw new NotSupportedException()
            };

            var appxReader = new AppxManifestReader();
            var pkg = await appxReader.Read(reader, cancellationToken).ConfigureAwait(false);
            cancellationToken.ThrowIfCancellationRequested();
            await LoadPackage(pkg, path, cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            this.Progress.IsLoading = originalProgress;
        }
    }

    public Task LoadPackage(InstalledPackage installedPackage, CancellationToken cancellationToken)
    {
        using var reader = FileReaderFactory.CreateFileReader(installedPackage.ManifestLocation);
        return LoadPackage(reader, cancellationToken);
    }
}