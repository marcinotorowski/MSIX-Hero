using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Otor.MsixHero.App.Controls.TransitionContentControl;
using Otor.MsixHero.App.Helpers;
using Otor.MsixHero.App.Hero;
using Otor.MsixHero.App.Modules.PackageManagement.PackageContent.Enums;
using Otor.MsixHero.App.Modules.PackageManagement.PackageContent.ViewModel.Capabilities;
using Otor.MsixHero.App.Modules.PackageManagement.PackageContent.ViewModel.Common;
using Otor.MsixHero.App.Modules.PackageManagement.PackageContent.ViewModel.Dependencies;
using Otor.MsixHero.App.Modules.PackageManagement.PackageContent.ViewModel.Files;
using Otor.MsixHero.App.Modules.PackageManagement.PackageContent.ViewModel.Installation;
using Otor.MsixHero.App.Modules.PackageManagement.PackageContent.ViewModel.Overview;
using Otor.MsixHero.App.Modules.PackageManagement.PackageContent.ViewModel.Overview.Actions;
using Otor.MsixHero.App.Modules.PackageManagement.PackageContent.ViewModel.Registry;
using Otor.MsixHero.App.Mvvm;
using Otor.MsixHero.Appx.Packaging;
using Otor.MsixHero.Appx.Packaging.Installation.Entities;
using Otor.MsixHero.Appx.Packaging.Installation.Enums;
using Otor.MsixHero.Appx.Packaging.Manifest;
using Otor.MsixHero.Appx.Packaging.Manifest.Entities;
using Otor.MsixHero.Appx.Packaging.Manifest.FileReaders;
using Otor.MsixHero.Elevation;
using Otor.MsixHero.Infrastructure.Helpers;
using Otor.MsixHero.Infrastructure.Services;
using Prism.Events;

namespace Otor.MsixHero.App.Modules.PackageManagement.PackageContent.ViewModel
{
    public class PackageContentViewModel : NotifyPropertyChanged, IPackageContentItemNavigation
    {
        private readonly IList<ILoadPackage> _loadPackageHandlers = new List<ILoadPackage>();
        private IPackageContentItem _currentItem;

        public PackageContentViewModel(IInteractionService interactionService,
            IConfigurationService configurationService,
            IEventAggregator eventAggregator,
            IUacElevation uacElevation,
            PrismServices prismServices, 
            IAppxFileViewer fileViewer, 
            FileInvoker fileInvoker)
        {
            this._loadPackageHandlers.Add(this.Actions = new ActionsViewModel(eventAggregator, configurationService));
            this.Items = new ObservableCollection<IPackageContentItem>();
            this.Header = new HeaderViewModel(this);

            var packageOverview = new PackageOverviewViewModel(this, interactionService, configurationService, eventAggregator, uacElevation, prismServices);
            var packageCapabilities = new PackageCapabilitiesViewModel(this);
            var packageDependencies = new PackageDependenciesViewModel(this);
            var packageInstallation = new PackageInstallationViewModel(this);
            var packageFiles = new PackageFilesViewModel(this, fileViewer, fileInvoker);
            var packageRegistry = new PackageRegistryViewModel(this);

            this.Items.Add(packageOverview);
            this.Items.Add(packageCapabilities);
            this.Items.Add(packageDependencies);
            this.Items.Add(packageInstallation);
            this.Items.Add(packageFiles);
            this.Items.Add(packageRegistry);

            this._loadPackageHandlers.Add(this.Header);
            this._loadPackageHandlers.Add(packageOverview);
            this._loadPackageHandlers.Add(packageCapabilities);
            this._loadPackageHandlers.Add(packageDependencies);
            this._loadPackageHandlers.Add(packageInstallation);
            this._loadPackageHandlers.Add(packageFiles);
            this._loadPackageHandlers.Add(packageRegistry);

            this.CurrentItem = packageOverview;
        }

        public HeaderViewModel Header { get; }

        public ProgressProperty Progress { get; } = new ProgressProperty();

        public ObservableCollection<IPackageContentItem> Items { get; }

        public void SetCurrentItem(PackageContentViewType type)
        {
            var page = this.Items.First(p => p.Type == type);
            this.CurrentItem = page;
        }

        public TransitioningContentControl.TransitionDirection TransitionDirection { get; private set; }

        public IPackageContentItem CurrentItem
        {
            get => _currentItem;
            set
            {
                if (this._currentItem == value)
                {
                    return;
                }

                if (this._currentItem != null)
                {
                    this._currentItem.IsActive = false;
                }

                if (value.Type == PackageContentViewType.Overview)
                {
                    this.TransitionDirection = TransitioningContentControl.TransitionDirection.Left;
                }
                else
                {
                    this.TransitionDirection = TransitioningContentControl.TransitionDirection.Right;
                }

                this.OnPropertyChanged(nameof(TransitionDirection));
                
                value.IsActive = true;

                this._currentItem = value;
                this.OnPropertyChanged(nameof(CurrentItem));
            }
        }

        public async Task LoadPackage(object objectToLoad)
        {
            if (objectToLoad is FileInfo fileInfo)
            {
                await this.LoadPackage(fileInfo.FullName).ConfigureAwait(false);
            }
            else if (objectToLoad is string filePathOrIdOrDirectory)
            {
                if (File.Exists(filePathOrIdOrDirectory))
                {
                    await this.LoadPackage(filePathOrIdOrDirectory).ConfigureAwait(false);
                }
                else if (Directory.Exists(filePathOrIdOrDirectory))
                {
                    using var reader = new DirectoryInfoFileReaderAdapter(filePathOrIdOrDirectory);
                    await this.LoadPackage(reader).ConfigureAwait(false);
                }
                else
                {
                    using var reader = new PackageIdentityFileReaderAdapter(PackageContext.CurrentUser, filePathOrIdOrDirectory);
                    await this.LoadPackage(reader).ConfigureAwait(false);
                }
            }
            else if (objectToLoad is IAppxFileReader fileReader)
            {
                await this.LoadPackage(fileReader).ConfigureAwait(false);
            }
            else if (objectToLoad is AppxPackage appxPackage)
            {
                await this.LoadPackage(appxPackage, appxPackage.Path).ConfigureAwait(false);
            }
            else if (objectToLoad is InstalledPackage installedPackage)
            {
                using var reader = new PackageIdentityFileReaderAdapter(PackageContext.CurrentUser, installedPackage.PackageFullName);
                await this.LoadPackage(reader).ConfigureAwait(false);
            }
            else
            {
                throw new NotSupportedException("Object of type " + objectToLoad.GetType() + " is not supported.");
            }
        }

        public async Task LoadPackage(string filePath)
        {
            using var reader = FileReaderFactory.CreateFileReader(filePath);
            await this.LoadPackage(reader);
        }

        public async Task LoadPackage(IAppxFileReader fileReader)
        {
            var manifestReader = new AppxManifestReader();
            var pkg = await manifestReader.Read(fileReader).ConfigureAwait(false);

            var path = fileReader switch
            {
                FileInfoFileReaderAdapter file => file.FilePath,
                IAppxDiskFileReader diskFileReader => Path.Combine(diskFileReader.RootDirectory, FileConstants.AppxManifestFile),
                ZipArchiveFileReaderAdapter zipReader => zipReader.PackagePath,
                _ => throw new NotSupportedException()
            };

            await this.LoadPackage(pkg, path).ConfigureAwait(false);
        }

        public async Task LoadPackage(AppxPackage model, string filePath)
        {
            var originalProgress = this.Progress.IsLoading;
            this.Progress.IsLoading = true;
            try
            {
                this.Logo = model.Logo;
                this.DisplayName = model.DisplayName;

                this.TileColor = null;

                if (model.Applications != null)
                {
                    foreach (var item in model.Applications)
                    {
                        if (string.IsNullOrEmpty(this.TileColor))
                        {
                            this.TileColor = item.BackgroundColor;
                            break;
                        }
                    }
                }

                if (string.IsNullOrEmpty(this.TileColor))
                {
                    this.TileColor = "#666666";
                }

                this.OnPropertyChanged(nameof(this.DisplayName));
                this.OnPropertyChanged(nameof(this.TileColor));
                this.OnPropertyChanged(nameof(this.Logo));

                await Task.WhenAll(this._loadPackageHandlers.Select(t => t.LoadPackage(model, filePath))).ConfigureAwait(false);
            }
            finally
            {
                this.Progress.IsLoading = originalProgress;
            }
        }
        
        public ActionsViewModel Actions { get; }
        
        public string DisplayName { get; private set; }

        public byte[] Logo { get; private set; }

        public string TileColor { get; private set; }
    }
}
