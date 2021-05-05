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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Otor.MsixHero.App.Controls.PackageExpert.ViewModels.Items;
using Otor.MsixHero.App.Controls.PackageExpert.ViewModels.Items.Content.Files;
using Otor.MsixHero.App.Controls.PackageExpert.ViewModels.Items.Content.Registry;
using Otor.MsixHero.App.Controls.PsfContent.ViewModel;
using Otor.MsixHero.App.Helpers;
using Otor.MsixHero.App.Modules.PackageManagement.PackageList.ViewModels;
using Otor.MsixHero.App.Mvvm;
using Otor.MsixHero.Appx.Packaging;
using Otor.MsixHero.Appx.Packaging.Installation;
using Otor.MsixHero.Appx.Packaging.Installation.Enums;
using Otor.MsixHero.Appx.Packaging.Manifest;
using Otor.MsixHero.Appx.Packaging.Manifest.Entities;
using Otor.MsixHero.Appx.Packaging.Manifest.FileReaders;
using Otor.MsixHero.Appx.Psf.Entities;
using Otor.MsixHero.Appx.Signing;
using Otor.MsixHero.Appx.Users;
using Otor.MsixHero.Appx.Volumes;
using Otor.MsixHero.Infrastructure.Helpers;
using Otor.MsixHero.Infrastructure.Processes;
using Otor.MsixHero.Infrastructure.Processes.SelfElevation;
using Otor.MsixHero.Infrastructure.Processes.SelfElevation.Enums;
using Otor.MsixHero.Infrastructure.Progress;
using Otor.MsixHero.Infrastructure.Services;
using Otor.MsixHero.Lib.Domain.State;
using Prism.Commands;

namespace Otor.MsixHero.App.Controls.PackageExpert.ViewModels
{
    public class PackageExpertViewModel : NotifyPropertyChanged
    {
        private readonly IInterProcessCommunicationManager interProcessCommunicationManager;
        private readonly ISelfElevationProxyProvider<IAppxPackageManager> appxPackageManagerProvider;
        private readonly ISelfElevationProxyProvider<IAppxVolumeManager> appxVolumeManagerProvider;
        private readonly string packagePath;
        private ICommand findUsers;

        public PackageExpertViewModel(
            string packagePath,
            IInterProcessCommunicationManager interProcessCommunicationManager,
            ISelfElevationProxyProvider<IAppxPackageManager> appxPackageManagerProvider,
            ISelfElevationProxyProvider<IAppxVolumeManager> appxVolumeManagerProvider,
            ISelfElevationProxyProvider<ISigningManager> signManager,
            IInteractionService interactionService)
        {
            this.interProcessCommunicationManager = interProcessCommunicationManager;
            this.appxPackageManagerProvider = appxPackageManagerProvider;
            this.appxVolumeManagerProvider = appxVolumeManagerProvider;
            this.packagePath = packagePath;

            this.Trust = new TrustViewModel(packagePath, interactionService, signManager);
        }
        
        public ProgressProperty Progress { get; } = new ProgressProperty();

        public async Task Load()
        {
            using var cts = new CancellationTokenSource();
            using var reader = FileReaderFactory.CreateFileReader(this.packagePath);
            var canElevate = await UserHelper.IsAdministratorAsync(cts.Token) || await this.interProcessCommunicationManager.Test(cts.Token);
            var progress = new Progress<ProgressData>();

            // Load manifest
            var taskLoadPackage = this.LoadManifest(reader, cts.Token);

            // Load certificate trust
            var taskLoadSignature = this.Trust.LoadSignature(cts.Token);

            // Load PSF
            var manifest = await taskLoadPackage.ConfigureAwait(false);
            var taskPsf = this.LoadPsf(reader, manifest);
                    
            // Load add-ons
            var taskAddOns = this.GetAddOns(manifest, cts.Token);

            // Load drive
            // var taskDisk = this.Disk.Load(this.LoadDisk());

            // Load users
            Task<FoundUsersViewModel> taskUsers;
            try
            {
                taskUsers = this.GetUsers(manifest, canElevate, cts.Token);
            }
            catch (Exception)
            {
                taskUsers = this.GetUsers(manifest, false, cts.Token);
            }

            await this.Manifest.Load(Task.FromResult(new PackageExpertPropertiesViewModel(manifest, this.packagePath))).ConfigureAwait(false);
            await this.PackageSupportFramework.Load(taskPsf).ConfigureAwait(false);
            await this.AddOns.Load(taskAddOns).ConfigureAwait(false);
            await this.Users.Load(taskUsers).ConfigureAwait(false);

            this.Registry = new AppxRegistryViewModel(this.packagePath);
            this.OnPropertyChanged(nameof(this.Registry));
            
            this.Files = new AppxFilesViewModel(this.packagePath);
            this.OnPropertyChanged(nameof(this.Files));
            
            // Wait for them all
            var allTasks = Task.WhenAll(taskLoadPackage, taskLoadSignature, taskPsf, taskAddOns, taskUsers);
            this.Progress.MonitorProgress(allTasks, cts, progress);
            await allTasks;
        }

        public async Task<PackageDriveViewModel> LoadDisk()
        {
            var disk = new PackageDriveViewModel(this.appxVolumeManagerProvider);
            await disk.Load(this.packagePath);
            return disk;
        }

        public TrustViewModel Trust { get; }

        public AppxRegistryViewModel Registry { get; private set; }
        
        public AppxFilesViewModel Files { get; private set; }
        
        public AsyncProperty<PackageDriveViewModel> Disk { get; } = new AsyncProperty<PackageDriveViewModel>();

        public AsyncProperty<PackageExpertPropertiesViewModel> Manifest { get; } = new AsyncProperty<PackageExpertPropertiesViewModel>();

        public AsyncProperty<PsfContentViewModel> PackageSupportFramework { get; } = new AsyncProperty<PsfContentViewModel>();

        public AsyncProperty<List<InstalledPackageViewModel>> AddOns { get; } = new AsyncProperty<List<InstalledPackageViewModel>>();

        public AsyncProperty<FoundUsersViewModel> Users { get; } = new AsyncProperty<FoundUsersViewModel>();
        
        public ICommand FindUsers
        {
            get
            {
                return this.findUsers ??= new DelegateCommand(
                    async () =>
                    {
                       
#pragma warning disable 4014
                        using (var reader = FileReaderFactory.CreateFileReader(this.packagePath))
                        {
                            var manifest = await this.LoadManifest(reader).ConfigureAwait(false);
                            await this.Users.Load(this.GetUsers(manifest, true)).ConfigureAwait(false);
                        }
#pragma warning restore 4014
                    });
            }
        }
        private async Task<FoundUsersViewModel> GetUsers(
            AppxPackage package,
            bool forceElevation, 
            CancellationToken cancellationToken = default, 
            IProgress<ProgressData> progress = null)
        {
            if (!forceElevation)
            {
                if (!await UserHelper.IsAdministratorAsync(cancellationToken))
                {
                    return new FoundUsersViewModel(new List<User>(), ElevationStatus.ElevationRequired);
                }
            }

            try
            {
                var manager = await this.appxPackageManagerProvider.GetProxyFor(forceElevation ? SelfElevationLevel.AsAdministrator : SelfElevationLevel.HighestAvailable, cancellationToken);

                var stateDetails = await manager.GetUsersForPackage(package.FullName, cancellationToken, progress).ConfigureAwait(false);

                if (stateDetails == null)
                {
                    return new FoundUsersViewModel(new List<User>(), ElevationStatus.ElevationRequired);
                }

                return new FoundUsersViewModel(stateDetails, ElevationStatus.OK);
            }
            catch (Exception)
            {
                return new FoundUsersViewModel(new List<User>(), ElevationStatus.ElevationRequired);
            }
        }

        private async Task<List<InstalledPackageViewModel>> GetAddOns(AppxPackage package, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = null)
        {
            var manager = await this.appxPackageManagerProvider.GetProxyFor(SelfElevationLevel.HighestAvailable, cancellationToken);
            var results = await manager.GetModificationPackages(package.FullName, PackageFindMode.Auto, cancellationToken, progress).ConfigureAwait(false);

            var list = new List<InstalledPackageViewModel>();
            foreach (var item in results)
            {
                list.Add(new InstalledPackageViewModel(item));
            }

            return list;
        }
        
        // ReSharper disable once MemberCanBeMadeStatic.Local
        private async Task<PsfContentViewModel> LoadPsf(IAppxFileReader source, AppxPackage manifest)
        {
            var paths = manifest.Applications.Where(a => PackageTypeConverter.GetPackageTypeFrom(a.EntryPoint, a.Executable, a.StartPage, manifest.IsFramework) == MsixPackageType.BridgePsf).Select(a => a.Executable).Where(a => a != null).Select(Path.GetDirectoryName).Where(a => !string.IsNullOrEmpty(a)).Distinct().ToList();
            paths.Add(string.Empty);

            foreach (var items in paths)
            {
                var configJsonPath = string.IsNullOrWhiteSpace(items)
                    ? "config.json"
                    : Path.Combine(items, "config.json");
                if (!source.FileExists(configJsonPath))
                {
                    continue;
                }

                using (var s = source.GetFile(configJsonPath))
                {
                    using var stringReader = new StreamReader(s);
                    var all = await stringReader.ReadToEndAsync().ConfigureAwait(false);
                    var psfSerializer = new PsfConfigSerializer();

                    var configJson = psfSerializer.Deserialize(all);
                    return new PsfContentViewModel(configJson);
                }
            }

            return null;
        }

        // ReSharper disable once MemberCanBeMadeStatic.Local
        private async Task<AppxPackage> LoadManifest(IAppxFileReader fileReader, CancellationToken cancellation = default)
        {
            var manifestReader = new AppxManifestReader();
            var manifest = await manifestReader.Read(fileReader, cancellation).ConfigureAwait(false);
            return manifest;
        }
    }
}
