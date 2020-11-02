using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Otor.MsixHero.Appx.Diagnostic.RunningDetector;
using Otor.MsixHero.Appx.Packaging;
using Otor.MsixHero.Appx.Packaging.Installation;
using Otor.MsixHero.Appx.Packaging.Installation.Enums;
using Otor.MsixHero.Appx.Packaging.Manifest;
using Otor.MsixHero.Appx.Packaging.Manifest.Entities;
using Otor.MsixHero.Appx.Packaging.Manifest.FileReaders;
using Otor.MsixHero.Appx.Psf.Entities;
using Otor.MsixHero.Appx.Signing;
using Otor.MsixHero.Appx.Signing.Entities;
using Otor.MsixHero.Appx.Users;
using Otor.MsixHero.Infrastructure.Helpers;
using Otor.MsixHero.Infrastructure.Logging;
using Otor.MsixHero.Infrastructure.Processes;
using Otor.MsixHero.Infrastructure.Processes.SelfElevation;
using Otor.MsixHero.Infrastructure.Processes.SelfElevation.Enums;
using Otor.MsixHero.Infrastructure.Progress;
using Otor.MsixHero.Infrastructure.Services;
using Otor.MsixHero.Lib.Domain.State;
using Otor.MsixHero.Ui.Helpers;
using Otor.MsixHero.Ui.Modules.Common.PackageContent.Helpers;
using Otor.MsixHero.Ui.Modules.Common.PackageContent.ViewModel.Elements;
using Otor.MsixHero.Ui.Modules.Common.PsfContent.ViewModel;
using Otor.MsixHero.Ui.Modules.PackageList.Navigation;
using Otor.MsixHero.Ui.Modules.PackageList.ViewModel.Elements;
using Otor.MsixHero.Ui.ViewModel;
using Prism.Commands;
using Prism.Regions;
using Prism.Services.Dialogs;

namespace Otor.MsixHero.Ui.Modules.Common.PackageContent.ViewModel
{
    public class PackageContentViewModel : NotifyPropertyChanged, INavigationAware, IObserver<ActivePackageFullNames>
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(PsfContentViewModel));

        private readonly IInterProcessCommunicationManager interProcessCommunicationManager;
        private readonly ISelfElevationProxyProvider<IAppxPackageManager> appxPackageManagerProvider;
        private readonly ISelfElevationProxyProvider<ISigningManager> signManager;
        private readonly IInteractionService interactionService;
        private readonly IRunningDetector runningDetector;
        private readonly IDialogService dialogService;
        private ICommand findUsers;
        private ICommand trustMe;
        private ICommand showPropertiesCommand;
        private string fullPackageName;
        private bool isTrusting;

        private string certificateFile;
        private IAppxFileReader packageSource;
        private IDisposable disposableSubscriber;
        private bool firstLoading = true;
        private ICommand visualize;

        public PackageContentViewModel(
            IInterProcessCommunicationManager interProcessCommunicationManager,
            ISelfElevationProxyProvider<IAppxPackageManager> appxPackageManagerProvider,
            ISelfElevationProxyProvider<ISigningManager> signManager,
            IInteractionService interactionService, 
            IConfigurationService configurationService,
            IRunningDetector runningDetector,
            IDialogService dialogService)
        {
            this.interProcessCommunicationManager = interProcessCommunicationManager;
            this.appxPackageManagerProvider = appxPackageManagerProvider;
            this.signManager = signManager;
            this.interactionService = interactionService;
            this.runningDetector = runningDetector;
            this.dialogService = dialogService;
            this.CommandHandler = new PackageContentCommandHandler(configurationService, interactionService);
        }

        public bool FirstLoading
        {
            get => this.firstLoading;
            set => this.SetField(ref this.firstLoading, value);
        }

        public PackageContentCommandHandler CommandHandler { get; }

        public AsyncProperty<FoundUsersViewModel> SelectedPackageUsersInfo { get; } = new AsyncProperty<FoundUsersViewModel>();
        
        public AsyncProperty<List<InstalledPackageViewModel>> Addons { get; } = new AsyncProperty<List<InstalledPackageViewModel>>();

        public AsyncProperty<PackageContentDetailsViewModel> SelectedPackageManifestInfo { get; } = new AsyncProperty<PackageContentDetailsViewModel>(isLoading: true);

        public AsyncProperty<PsfContentViewModel> SelectedPackageJsonInfo { get; } = new AsyncProperty<PsfContentViewModel>(isLoading: true);

        public AsyncProperty<TrustStatus> TrustStatus { get; } = new AsyncProperty<TrustStatus>();

        public bool IsTrusting
        {
            get => this.isTrusting;
            set => this.SetField(ref this.isTrusting, value);
        }
        
        public ICommand TrustMeCommand
        {
            get
            {
                return this.trustMe ??= new DelegateCommand(async () =>
                {
                    if (this.interactionService.Confirm("Are you sure you want to add this publisher to the list of trusted publishers (machine-wide)?", type: InteractionType.Question, buttons:InteractionButton.YesNo) != InteractionResult.Yes)
                    {
                        return;
                    }
                    
                    try
                    {
                        this.IsTrusting = true;
                    
                        var manager = await this.signManager.GetProxyFor(SelfElevationLevel.AsAdministrator).ConfigureAwait(false);
                        await manager.Trust(this.certificateFile).ConfigureAwait(false);
                        await this.TrustStatus.Load(this.LoadSignature(this.packageSource, CancellationToken.None)).ConfigureAwait(false);
                    }
                    finally
                    {
                        this.IsTrusting = false;
                    }
                },
                () => this.certificateFile != null);
            }
        }
        
        public ICommand ShowPropertiesCommand
        {
            get
            {
                return this.showPropertiesCommand ??= new DelegateCommand(() =>
                {
                    WindowsExplorerCertificateHelper.ShowFileSecurityProperties(this.certificateFile, IntPtr.Zero);
                }, () => this.certificateFile != null);
            }
        }

        public ICommand FindUsers
        {
            get
            {
                return this.findUsers ??= new DelegateCommand(
                    () =>
                    {
                        if (this.fullPackageName == null)
                        {
                            return;
                        }

#pragma warning disable 4014
                        this.SelectedPackageUsersInfo.Load(this.GetSelectionDetails(true));
#pragma warning restore 4014
                    });
            }
        }

        public ICommand Visualize
        {
            get
            {
                return this.visualize ??= new DelegateCommand(this.VisualizeExecuted);
            }
        }

        private void VisualizeExecuted()
        {
            string file;
            if (this.packageSource is ZipArchiveFileReaderAdapter zip)
            {
                file = zip.PackagePath;
            }
            else if (this.packageSource is FileInfoFileReaderAdapter manifest)
            {
                file = Path.Combine(manifest.RootDirectory, "AppxManifest.xml");
            }
            else
            {
                return;
            }

            this.dialogService.ShowDialog(Constants.PathDependencyViewer, new DialogParameters() { { "file", file } }, o => { });
        }

        private async Task<TrustStatus> LoadSignature(IAppxFileReader source, CancellationToken cancellationToken)
        {
            if (source is ZipArchiveFileReaderAdapter zipFileReader)
            {
                this.certificateFile = zipFileReader.PackagePath;
                var manager = await this.signManager.GetProxyFor(SelfElevationLevel.AsInvoker, cancellationToken).ConfigureAwait(false);
                var signTask = manager.IsTrusted(zipFileReader.PackagePath, cancellationToken);
                await this.TrustStatus.Load(signTask);
                return await signTask.ConfigureAwait(false);
            }

            if (source is IAppxDiskFileReader fileReader)
            {
                var file = new FileInfo(Path.Combine(fileReader.RootDirectory, "AppxSignature.p7x"));
                if (file.Exists)
                {
                    this.certificateFile = file.FullName;
                    var manager = await this.signManager.GetProxyFor(SelfElevationLevel.AsInvoker, cancellationToken).ConfigureAwait(false);
                    var signTask = manager.IsTrusted(file.FullName, cancellationToken);
                    await this.TrustStatus.Load(signTask);
                    return await signTask.ConfigureAwait(false);
                }
            }

            return new TrustStatus();
        }

        public async Task<PackageContentDetailsViewModel> LoadPackage(IAppxFileReader source, CancellationToken cancellationToken = default)
        {
            this.certificateFile = null;
            this.packageSource = source;
            if (!source.FileExists("AppxManifest.xml"))
            {
                throw new FileNotFoundException("Could not find AppxManifest.xml");
            }
            
            var loadSignature = this.LoadSignature(source, cancellationToken);

            var appxReader = new AppxManifestReader();

            var taskToReadManifest = appxReader.Read(source, cancellationToken);
            
            var appxManifest = await taskToReadManifest.ConfigureAwait(false);
            this.fullPackageName = appxManifest.FullName;

            Task loadUsers;
            var canElevate = await UserHelper.IsAdministratorAsync(cancellationToken) || await this.interProcessCommunicationManager.Test(cancellationToken);
            try
            {
                loadUsers = this.SelectedPackageUsersInfo.Load(this.GetSelectionDetails(canElevate, cancellationToken));
            }
            catch (Exception)
            {
                loadUsers = this.SelectedPackageUsersInfo.Load(this.GetSelectionDetails(false, cancellationToken));
            }

            var getAddons = this.Addons.Load(this.GetAddons(cancellationToken));
            await Task.WhenAll(getAddons, loadUsers, loadSignature).ConfigureAwait(false);

            this.FirstLoading = false;

            if (source is IAppxDiskFileReader diskReader)
            {
                return new PackageContentDetailsViewModel(appxManifest, diskReader.RootDirectory);
            }

            return new PackageContentDetailsViewModel(appxManifest);
        }

        public async Task<PsfContentViewModel> LoadPackageJson(IAppxFileReader source, AppxPackage manifest, CancellationToken cancellationToken = default)
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

                using var stringReader = new StreamReader(source.GetFile(configJsonPath));
                var all = await stringReader.ReadToEndAsync().ConfigureAwait(false);
                var psfSerializer = new PsfConfigSerializer();

                var configJson = psfSerializer.Deserialize(all);
                return new PsfContentViewModel(configJson);
            }

            return null;
        }

        void IObserver<ActivePackageFullNames>.OnCompleted()
        {
        }

        void IObserver<ActivePackageFullNames>.OnError(Exception error)
        {
        }

        void IObserver<ActivePackageFullNames>.OnNext(ActivePackageFullNames value)
        {
        }

        async void INavigationAware.OnNavigatedTo(NavigationContext navigationContext)
        {
            var navigation = new PackageListNavigation(navigationContext);
            if (navigation.SelectedManifests?.Count != 1)
            {
                return;
            }

            this.disposableSubscriber?.Dispose();
            this.disposableSubscriber = this.runningDetector.Subscribe(this);

            var manifest = navigation.SelectedManifests.First();

            using IAppxFileReader fileReader = new FileInfoFileReaderAdapter(manifest);
            var task = this.LoadPackage(fileReader, CancellationToken.None);

            try
            {
                await this.SelectedPackageManifestInfo.Load(task).ConfigureAwait(false);
            }
            catch (Exception)
            {
                Logger.Error($"The package {manifest} could not be loaded.");
                await this.SelectedPackageJsonInfo.Load(Task.FromResult(default(PsfContentViewModel))).ConfigureAwait(false);
                return;
            }

            if (task.IsFaulted || task.IsCanceled)
            {
                Logger.Error($"The package {manifest} could not be loaded.");
                await this.SelectedPackageJsonInfo.Load(Task.FromResult(default(PsfContentViewModel))).ConfigureAwait(false);
                return;
            }

            try
            {
                var details = await task.ConfigureAwait(false);
                await this.SelectedPackageJsonInfo.Load(this.LoadPackageJson(fileReader, details.Model, CancellationToken.None)).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                Logger.Error(e, $"The package {manifest} could not be loaded.");
                await this.SelectedPackageJsonInfo.Load(Task.FromResult(default(PsfContentViewModel))).ConfigureAwait(false);
            }
        }

        bool INavigationAware.IsNavigationTarget(NavigationContext navigationContext)
        {
            var navigation = new PackageListNavigation(navigationContext);
            return navigation.SelectedManifests?.Count == 1;
        }

        void INavigationAware.OnNavigatedFrom(NavigationContext navigationContext)
        {
            this.disposableSubscriber?.Dispose();
        }

        private async Task<List<InstalledPackageViewModel>> GetAddons(CancellationToken cancellationToken = default, IProgress<ProgressData> progress = null)
        {
            var manager = await this.appxPackageManagerProvider.GetProxyFor(SelfElevationLevel.HighestAvailable, cancellationToken);
            var results = await manager.GetModificationPackages(this.fullPackageName, PackageFindMode.Auto, cancellationToken, progress).ConfigureAwait(false);
            
            var list = new List<InstalledPackageViewModel>();
            foreach (var item in results)
            {
                list.Add(new InstalledPackageViewModel(item));
            }

            return list;
        }

        private async Task<FoundUsersViewModel> GetSelectionDetails(bool forceElevation, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = null)
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
                
                var stateDetails = await manager.GetUsersForPackage(this.fullPackageName, cancellationToken, progress).ConfigureAwait(false);

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
    }
}
