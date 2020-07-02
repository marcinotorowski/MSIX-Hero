using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using otor.msixhero.lib.BusinessLayer.Appx.Manifest;
using otor.msixhero.lib.BusinessLayer.Appx.Manifest.FileReaders;
using otor.msixhero.lib.BusinessLayer.Helpers;
using otor.msixhero.lib.BusinessLayer.Managers.Signing;
using otor.msixhero.lib.BusinessLayer.State;
using otor.msixhero.lib.Domain.Appx.Manifest.Full;
using otor.msixhero.lib.Domain.Appx.Packages;
using otor.msixhero.lib.Domain.Appx.Psf;
using otor.msixhero.lib.Domain.Appx.Signing;
using otor.msixhero.lib.Domain.Appx.Users;
using otor.msixhero.lib.Domain.Commands.Packages.Grid;
using otor.msixhero.lib.Domain.Commands.Packages.Signing;
using otor.msixhero.lib.Domain.State;
using otor.msixhero.lib.Infrastructure;
using otor.msixhero.lib.Infrastructure.Configuration;
using otor.msixhero.lib.Infrastructure.Logging;
using otor.msixhero.lib.Infrastructure.SelfElevation;
using otor.msixhero.ui.Helpers;
using otor.msixhero.ui.Modules.Common.PackageContent.Helpers;
using otor.msixhero.ui.Modules.Common.PackageContent.ViewModel.Elements;
using otor.msixhero.ui.Modules.Common.PsfContent.ViewModel;
using otor.msixhero.ui.Modules.PackageList.Navigation;
using otor.msixhero.ui.ViewModel;
using Prism.Commands;
using Prism.Regions;

namespace otor.msixhero.ui.Modules.Common.PackageContent.ViewModel
{
    public class PackageContentViewModel : NotifyPropertyChanged, INavigationAware
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(PsfContentViewModel));

        private readonly IApplicationStateManager stateManager;
        private readonly ISelfElevationManagerFactory<ISigningManager> signManager;
        private readonly IInteractionService interactionService;
        private ICommand findUsers;
        private ICommand trustMe;
        private ICommand showPropertiesCommand;
        private string fullPackageName;
        private bool isTrusting;

        private string certificateFile;
        private IAppxFileReader packageSource;

        public PackageContentViewModel(
            IApplicationStateManager stateManager,
            ISelfElevationManagerFactory<ISigningManager> signManager,
            IInteractionService interactionService, 
            IConfigurationService configurationService)
        {
            this.stateManager = stateManager;
            this.signManager = signManager;
            this.interactionService = interactionService;
            this.CommandHandler = new PackageContentCommandHandler(configurationService, interactionService);
        }

        public PackageContentCommandHandler CommandHandler { get; }

        public AsyncProperty<FoundUsersViewModel> SelectedPackageUsersInfo { get; } = new AsyncProperty<FoundUsersViewModel>();

        public AsyncProperty<PackageContentDetailsViewModel> SelectedPackageManifestInfo { get; } = new AsyncProperty<PackageContentDetailsViewModel>();

        public AsyncProperty<PsfContentViewModel> SelectedPackageJsonInfo { get; } = new AsyncProperty<PsfContentViewModel>();
        
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

                        await this.stateManager.CommandExecutor.ExecuteAsync(new TrustPublisher(this.certificateFile), CancellationToken.None).ConfigureAwait(false);
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

        private async Task<TrustStatus> LoadSignature(IAppxFileReader source, CancellationToken cancellationToken)
        {
            if (source is ZipArchiveFileReaderAdapter zipFileReader)
            {
                this.certificateFile = zipFileReader.PackagePath;
                var manager = await this.signManager.Get(SelfElevationLevel.AsInvoker, cancellationToken).ConfigureAwait(false);
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
                    var manager = await this.signManager.Get(SelfElevationLevel.AsInvoker, cancellationToken).ConfigureAwait(false);
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

            var signatureTask = this.LoadSignature(source, cancellationToken);

            var appxReader = new AppxManifestReader();
            var appxManifest = await appxReader.Read(source, cancellationToken).ConfigureAwait(false);

            this.fullPackageName = appxManifest.FullName;
            if (!this.stateManager.CurrentState.IsElevated && !this.stateManager.CurrentState.IsSelfElevated)
            {
                await this.SelectedPackageUsersInfo.Load(Task.FromResult(new FoundUsersViewModel(new List<User>(), ElevationStatus.ElevationRequired))).ConfigureAwait(false);
            }
            else
            {
                try
                {
                    await this.SelectedPackageUsersInfo.Load(this.GetSelectionDetails(this.stateManager.CurrentState.IsElevated || this.stateManager.CurrentState.IsSelfElevated)).ConfigureAwait(false);
                }
                catch (Exception)
                {
                    await this.SelectedPackageUsersInfo.Load(this.GetSelectionDetails(false)).ConfigureAwait(false);
                }
            }

            await signatureTask.ConfigureAwait(false);
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

        async void INavigationAware.OnNavigatedTo(NavigationContext navigationContext)
        {
            var navigation = new PackageListNavigation(navigationContext);
            if (navigation.SelectedManifests?.Count != 1)
            {
                return;
            }

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
        }

        private async Task<FoundUsersViewModel> GetSelectionDetails(bool forceElevation)
        {
            try
            {
                var stateDetails = await this.stateManager.CommandExecutor.GetExecuteAsync(new FindUsers(this.fullPackageName, forceElevation)).ConfigureAwait(false);
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
