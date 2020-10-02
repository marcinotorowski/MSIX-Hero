using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Otor.MsixHero.AppInstaller.Entities;
using Otor.MsixHero.Appx.Packaging.Manifest.Enums;
using Otor.MsixHero.Appx.Packaging.ModificationPackages;
using Otor.MsixHero.Appx.Packaging.ModificationPackages.Entities;
using Otor.MsixHero.Appx.Signing;
using Otor.MsixHero.Infrastructure.Configuration;
using Otor.MsixHero.Infrastructure.Processes.SelfElevation;
using Otor.MsixHero.Infrastructure.Processes.SelfElevation.Enums;
using Otor.MsixHero.Infrastructure.Progress;
using Otor.MsixHero.Infrastructure.Services;
using Otor.MsixHero.Ui.Commands.RoutedCommand;
using Otor.MsixHero.Ui.Controls.ChangeableDialog.ViewModel;
using Otor.MsixHero.Ui.Domain;
using Otor.MsixHero.Ui.Modules.Dialogs.Common.CertificateSelector.ViewModel;
using Otor.MsixHero.Ui.Modules.Dialogs.Common.PackageSelector.ViewModel;
using Prism.Services.Dialogs;

namespace Otor.MsixHero.Ui.Modules.Dialogs.ModificationPackage.ViewModel
{
    public class ModificationPackageViewModel : ChangeableDialogViewModel, IDialogAware
    {
        private readonly IAppxContentBuilder contentBuilder;
        private readonly ISelfElevationProxyProvider<ISigningManager> signingManagerFactory;
        private readonly IConfigurationService configurationService;
        private readonly IInteractionService interactionService;
        private ICommand openSuccessLink;
        private ICommand reset;
        
        public ModificationPackageViewModel(
            IAppxContentBuilder contentBuilder,
            ISelfElevationProxyProvider<ISigningManager> signingManagerFactory,
            IConfigurationService configurationService,
            IInteractionService interactionService) : base("Create modification package", interactionService)
        {
            this.contentBuilder = contentBuilder;
            this.signingManagerFactory = signingManagerFactory;
            this.configurationService = configurationService;
            this.interactionService = interactionService;
            
            this.InitializeTabProperties();
            this.InitializeTabParentPackage();
            this.InitializeTabContent();
            this.InitializeTabCertificate();

            this.AddChildren(
                this.TabProperties,
                this.TabParentPackage,
                this.TabContent,
                this.TabCertificate);
        }

        public ChangeableFileProperty SourcePath { get; private set; }

        public PackageSelectorViewModel PackageSelection { get; private set; }

        public PackageSelectorViewModel TabProperties { get; private set; }

        public ChangeableProperty<bool> IncludeFiles { get; private set; }

        public ChangeableContainer TabParentPackage { get; private set; }

        public ChangeableContainer TabContent { get; private set; }

        public ChangeableProperty<PackageSourceMode> PackageSourceMode { get; private set; }

        public ChangeableProperty<bool> IncludeRegistry { get; private set; }

        public ChangeableProperty<bool> IncludeVfsFolders { get; private set; }

        public ChangeableFileProperty SourceRegistryFile { get; private set; }

        public ChangeableFolderProperty SourceFolder { get; private set; }

        public ChangeableProperty<ModificationPackageBuilderAction> Create { get; private set; }

        public ICommand OpenSuccessLinkCommand
        {
            get { return this.openSuccessLink ??= new DelegateCommand(this.OpenSuccessLinkExecuted); }
        }

        public ICommand ResetCommand
        {
            get { return this.reset ??= new DelegateCommand(this.ResetExecuted); }
        }

        public string Result { get; private set; }

        public bool IsIncludeVfsFoldersEnabled => !string.IsNullOrEmpty(this.PackageSelection.InputPath.CurrentValue) && this.PackageSourceMode.CurrentValue == ViewModel.PackageSourceMode.FromFile && this.Create.CurrentValue == ModificationPackageBuilderAction.Manifest;

        public CertificateSelectorViewModel TabCertificate { get; private set; }
        
        public void OnDialogOpened(IDialogParameters parameters)
        {
            if (!parameters.TryGetValue("file", out string sourceFile))
            {
                return;
            }

            this.PackageSelection.InputPath.CurrentValue = sourceFile;
            this.PackageSelection.AllowChangingSourcePackage = false;
            this.PackageSelection.ShowPackageTypeSelector = false;

            this.TabProperties.Architecture.CurrentValue = this.PackageSelection.Architecture.CurrentValue;
            this.TabProperties.Name.CurrentValue = this.PackageSelection.Name.CurrentValue + "-Modification";
            this.TabProperties.Commit();
        }

        protected override async Task<bool> Save(CancellationToken cancellationToken, IProgress<ProgressData> progress)
        {
            if (!this.IsValid)
            {
                return false;
            }

            // ReSharper disable once NotAccessedVariable
            string selectedPath;

            switch (this.Create.CurrentValue)
            {
                case ModificationPackageBuilderAction.Manifest:
                    if (!this.interactionService.SelectFolder(out selectedPath))
                    {
                        return false;
                    }

                    selectedPath = Path.Join(selectedPath, "AppxManifest.xml");
                    break;

                case ModificationPackageBuilderAction.Msix:
                case ModificationPackageBuilderAction.SignedMsix:
                    if (!this.interactionService.SaveFile("MSIX Modification Packages|*.msix", out selectedPath))
                    {
                        return false;
                    }

                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (!string.IsNullOrEmpty(this.SourcePath.CurrentValue))
            {
                this.TabProperties.InputPath.CurrentValue = this.SourcePath.CurrentValue;
            }

            var modificationPkgCreationRequest = new ModificationPackageConfig
            {
                DisplayName = this.TabProperties.DisplayName.CurrentValue,
                Name = Regex.Replace(this.TabProperties.DisplayName.CurrentValue, "[^a-zA-Z0-9\\-]", string.Empty),
                Publisher = "CN=" + Regex.Replace(this.TabProperties.DisplayPublisher.CurrentValue, "[,=]", string.Empty),
                DisplayPublisher = this.TabProperties.DisplayPublisher.CurrentValue,
                Version = this.TabProperties.Version.CurrentValue,
                ParentName = this.PackageSelection.Name.CurrentValue,
                ParentPublisher = this.PackageSelection.Publisher.CurrentValue,
                IncludeVfsFolders = this.IncludeVfsFolders.CurrentValue && this.IsIncludeVfsFoldersEnabled,
                IncludeFolder = this.IncludeFiles.CurrentValue && !string.IsNullOrEmpty(this.SourceFolder.CurrentValue) ? new DirectoryInfo(this.SourceFolder.CurrentValue) : null,
                IncludeRegistry = this.IncludeRegistry.CurrentValue && !string.IsNullOrEmpty(this.SourceRegistryFile.CurrentValue) ? new FileInfo(this.SourceRegistryFile.CurrentValue) : null,
                ParentPackagePath = this.PackageSelection.InputPath.CurrentValue
            };

            modificationPkgCreationRequest.Architecture = (AppxPackageArchitecture) Enum.Parse(typeof(AppxPackageArchitecture), modificationPkgCreationRequest.Architecture.ToString("G"), true);

            await this.contentBuilder.Create(modificationPkgCreationRequest, selectedPath, this.Create.CurrentValue, cancellationToken, progress).ConfigureAwait(false);

            switch (this.Create.CurrentValue)
            {
                case ModificationPackageBuilderAction.Manifest:
                    this.Result = selectedPath;
                    break;
                case ModificationPackageBuilderAction.Msix:
                    this.Result = selectedPath;
                    break;
                case ModificationPackageBuilderAction.SignedMsix:

                    var manager = await this.signingManagerFactory.GetProxyFor(SelfElevationLevel.AsInvoker, cancellationToken).ConfigureAwait(false);
                    cancellationToken.ThrowIfCancellationRequested();

                    switch (this.TabCertificate.Store.CurrentValue)
                    {
                        case CertificateSource.Pfx:
                            await manager.SignPackage(selectedPath, true, this.TabCertificate.PfxPath.CurrentValue, this.TabCertificate.Password.CurrentValue, this.TabCertificate.TimeStamp.CurrentValue, IncreaseVersionMethod.None, cancellationToken, progress).ConfigureAwait(false);
                            break;
                        case CertificateSource.Personal:
                            await manager.SignPackage(selectedPath, true, this.TabCertificate.SelectedPersonalCertificate?.CurrentValue?.Model, this.TabCertificate.TimeStamp.CurrentValue, IncreaseVersionMethod.None,cancellationToken, progress).ConfigureAwait(false);
                            break;
                    }

                    this.Result = selectedPath;
                    break;
            }

            return true;
        }

        private void PackageFileChanged(object sender, PackageSelectorViewModel.PackageFileChangedEventArgs e)
        {
            this.OnPropertyChanged(nameof(IsIncludeVfsFoldersEnabled));
            this.TabProperties.Architecture.CurrentValue = e.Architecture;
        }

        private void ResetExecuted(object parameter)
        {
            this.State.IsSaved = false;
        }

        private void CreateOnValueChanged(object sender, ValueChangedEventArgs e)
        {
            this.TabCertificate.IsValidated = this.Create.CurrentValue == ModificationPackageBuilderAction.SignedMsix;
            this.OnPropertyChanged(nameof(IsIncludeVfsFoldersEnabled));
        }

        private void PackageSourceModeChanged(object sender, ValueChangedEventArgs e)
        {
            this.OnPropertyChanged(nameof(IsIncludeVfsFoldersEnabled));
            this.PackageSelection.IsValidated = (PackageSourceMode)e.NewValue == ViewModel.PackageSourceMode.FromProperties;
            this.SourcePath.IsValidated = (PackageSourceMode)e.NewValue == ViewModel.PackageSourceMode.FromFile;
        }

        private void OpenSuccessLinkExecuted(object parameter)
        {
            Process.Start("explorer.exe", "/select," + this.Result);
        }
        private void IncludeFilesChanged(object sender, ValueChangedEventArgs e)
        {
            this.SourceFolder.IsValidated = (bool)e.NewValue;
        }

        private void IncludeRegistryChanged(object sender, ValueChangedEventArgs e)
        {
            this.SourceRegistryFile.IsValidated = (bool)e.NewValue;
        }
        private void InitializeTabProperties()
        {
            this.TabProperties = new PackageSelectorViewModel(
                interactionService,
                PackageSelectorDisplayMode.AllowChanging |
                PackageSelectorDisplayMode.AllowPackages |
                PackageSelectorDisplayMode.RequireVersion |
                PackageSelectorDisplayMode.ShowDisplayName);

            this.TabProperties.Version.CurrentValue = "1.0.0.0";
            this.TabProperties.Architecture.CurrentValue = (AppxPackageArchitecture)Enum.Parse(typeof(AppxPackageArchitecture), AppInstallerPackageArchitecture.neutral.ToString("G"), true);
            this.TabProperties.Commit();
        }

        private void InitializeTabParentPackage()
        {
            this.SourcePath = new ChangeableFileProperty(this.interactionService)
            {
                DisplayName = "Parent package",
                Filter = "Packages|*.msix;*.appx;Appxmanifest.xml|All files|*.*",
                IsValidated = true,
                Validators = new[] { ChangeableFileProperty.ValidatePathAndPresence }
            };

            this.PackageSourceMode = new ChangeableProperty<PackageSourceMode>();
            this.PackageSourceMode.ValueChanged += this.PackageSourceModeChanged;

            this.PackageSelection = new PackageSelectorViewModel(
                interactionService,
                PackageSelectorDisplayMode.AllowChanging |
                PackageSelectorDisplayMode.AllowBundles |
                PackageSelectorDisplayMode.ShowTypeSelector |
                PackageSelectorDisplayMode.AllowPackages |
                PackageSelectorDisplayMode.AllowAllPackageTypes |
                PackageSelectorDisplayMode.AllowChanging |
                PackageSelectorDisplayMode.RequireFullIdentity |
                PackageSelectorDisplayMode.ShowActualName)
            {
                IsValidated = false
            };
            this.PackageSelection.PackageFileChanged += this.PackageFileChanged;

            this.TabParentPackage = new ChangeableContainer(
                this.PackageSourceMode,
                this.PackageSelection,
                this.SourcePath);
        }

        private void InitializeTabContent()
        {
            this.Create = new ChangeableProperty<ModificationPackageBuilderAction>();
            if (configurationService.GetCurrentConfiguration().Packer.SignByDefault)
            {
                this.Create.CurrentValue = ModificationPackageBuilderAction.SignedMsix;
            }

            this.IncludeFiles = new ChangeableProperty<bool>();
            this.IncludeRegistry = new ChangeableProperty<bool>();
            this.IncludeVfsFolders = new ChangeableProperty<bool>();

            this.SourceFolder = new ChangeableFolderProperty(this.interactionService)
            {
                DisplayName = "Folder to include",
                IsValidated = false,
                Validators = new[] { ChangeableFolderProperty.ValidatePathAndPresence }
            };

            this.SourceRegistryFile = new ChangeableFileProperty(this.interactionService)
            {
                DisplayName = ".REG file to include",
                IsValidated = false,
                Filter = "Registry files (*.reg)|*.reg",
                OpenForSaving = false,
                Validators = new[] { ChangeableFileProperty.ValidatePathAndPresence }
            };

            this.TabContent = new ChangeableContainer(
                this.Create,
                this.IncludeVfsFolders,
                this.IncludeFiles,
                this.SourceFolder,
                this.IncludeRegistry,
                this.SourceRegistryFile);

            this.Create.ValueChanged += this.CreateOnValueChanged;
            this.IncludeRegistry.ValueChanged += this.IncludeRegistryChanged;
            this.IncludeFiles.ValueChanged += this.IncludeFilesChanged;
        }

        private void InitializeTabCertificate()
        {
            this.TabCertificate = new CertificateSelectorViewModel(this.interactionService, this.signingManagerFactory, this.configurationService.GetCurrentConfiguration()?.Signing, true);
        }
    }
}

