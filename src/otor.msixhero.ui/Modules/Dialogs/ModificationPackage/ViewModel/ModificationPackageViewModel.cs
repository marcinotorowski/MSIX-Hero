using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using otor.msixhero.lib.BusinessLayer.Appx.Builder;
using otor.msixhero.lib.BusinessLayer.Appx.Signing;
using otor.msixhero.lib.Domain.Appx.AppInstaller;
using otor.msixhero.lib.Domain.Appx.ModificationPackage;
using otor.msixhero.lib.Infrastructure;
using otor.msixhero.lib.Infrastructure.Configuration;
using otor.msixhero.lib.Infrastructure.Progress;
using otor.msixhero.ui.Commands.RoutedCommand;
using otor.msixhero.ui.Controls.ChangeableDialog.ViewModel;
using otor.msixhero.ui.Domain;
using otor.msixhero.ui.Modules.Dialogs.Common.CertificateSelector.ViewModel;
using otor.msixhero.ui.Modules.Dialogs.Common.PackageSelector.ViewModel;
using Prism.Services.Dialogs;

namespace otor.msixhero.ui.Modules.Dialogs.ModificationPackage.ViewModel
{
    using System.Diagnostics;
    using System.Text.RegularExpressions;

    public class ModificationPackageViewModel : ChangeableDialogViewModel, IDialogAware
    {
        private readonly IAppxContentBuilder contentBuilder;
        private readonly IAppxSigningManager signingManager;
        private readonly IInteractionService interactionService;
        private ICommand openSuccessLink;
        private ICommand reset;

        public ModificationPackageViewModel(
            IAppxContentBuilder contentBuilder, 
            IAppxSigningManager signingManager,
            IConfigurationService configurationService,
            IInteractionService interactionService) : base("Create modification package stub", interactionService)
        {
            this.contentBuilder = contentBuilder;
            this.signingManager = signingManager;
            this.interactionService = interactionService;

            this.PackageSelection = new PackageSelectorViewModel(
                interactionService,
                PackageSelectorDisplayMode.AllowChanging | 
                PackageSelectorDisplayMode.AllowBundles | 
                PackageSelectorDisplayMode.ShowTypeSelector | 
                PackageSelectorDisplayMode.AllowPackages |
                PackageSelectorDisplayMode.AllowBrowsing | 
                PackageSelectorDisplayMode.AllowChanging | 
                PackageSelectorDisplayMode.RequireFullIdentity | 
                PackageSelectorDisplayMode.ShowActualName);

            this.ModificationPackageDetails = new PackageSelectorViewModel(
                interactionService,
                PackageSelectorDisplayMode.AllowChanging | 
                PackageSelectorDisplayMode.AllowPackages | 
                PackageSelectorDisplayMode.RequireFullIdentity | 
                PackageSelectorDisplayMode.ShowDisplayName);

            this.ModificationPackageDetails.Version.CurrentValue = "1.0.0.0";
            this.ModificationPackageDetails.Architecture.CurrentValue = AppInstallerPackageArchitecture.neutral;
            this.ModificationPackageDetails.Commit();
            
            this.Create = new ChangeableProperty<ModificationPackageBuilderAction>();
            this.Create.ValueChanged += this.CreateOnValueChanged;

            this.SelectedCertificate = new CertificateSelectorViewModel(interactionService, signingManager, configurationService.GetCurrentConfiguration()?.Signing, true);
            
            this.AddChildren(this.PackageSelection, this.ModificationPackageDetails, this.Create, this.SelectedCertificate);
            this.SetValidationMode(ValidationMode.Silent, true);
        }

        public PackageSelectorViewModel PackageSelection { get; }

        public PackageSelectorViewModel ModificationPackageDetails { get; }

        public ChangeableProperty<ModificationPackageBuilderAction> Create { get; }
        
        public ICommand OpenSuccessLinkCommand
        {
            get { return this.openSuccessLink ??= new DelegateCommand(this.OpenSuccessLinkExecuted); }
        }

        public ICommand ResetCommand
        {
            get { return this.reset ??= new DelegateCommand(this.ResetExecuted); }
        }

        public string Result { get; private set; }
        
        public CertificateSelectorViewModel SelectedCertificate { get; }
        
        public void OnDialogOpened(IDialogParameters parameters)
        {
            if (!parameters.TryGetValue("file", out string sourceFile))
            {
                return;
            }

            this.PackageSelection.InputPath.CurrentValue = sourceFile;
            this.PackageSelection.AllowChangingSourcePackage = false;
            this.PackageSelection.ShowPackageTypeSelector = false;

            this.ModificationPackageDetails.Architecture.CurrentValue = this.PackageSelection.Architecture.CurrentValue;
            this.ModificationPackageDetails.Name.CurrentValue = this.PackageSelection.Name.CurrentValue + "-Modification";
            this.ModificationPackageDetails.Commit();
        }

        protected override async Task<bool> Save(CancellationToken cancellationToken, IProgress<ProgressData> progress)
        {
            this.SetValidationMode(ValidationMode.Default, true);
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

            var modificationPkgCreationRequest = new ModificationPackageConfig
            {
                DisplayName = this.ModificationPackageDetails.DisplayName.CurrentValue,
                Name = Regex.Replace(this.ModificationPackageDetails.DisplayName.CurrentValue, "[^a-zA-Z0-9\\-]", string.Empty),
                Publisher = "CN=" + Regex.Replace(this.ModificationPackageDetails.DisplayPublisher.CurrentValue, "[,=]", string.Empty),
                DisplayPublisher = this.ModificationPackageDetails.DisplayPublisher.CurrentValue,
                Architecture = this.ModificationPackageDetails.Architecture.CurrentValue,
                Version = this.ModificationPackageDetails.Version.CurrentValue,
                ParentName = this.PackageSelection.Name.CurrentValue,
                ParentPublisher = this.PackageSelection.Publisher.CurrentValue
            };

            await this.contentBuilder.Create(modificationPkgCreationRequest, selectedPath, this.Create.CurrentValue, cancellationToken, progress).ConfigureAwait(false);

            switch (this.Create.CurrentValue)
            {
                case ModificationPackageBuilderAction.Manifest:
                    this.Result = Path.Combine(selectedPath, "AppxManifest.xml");
                    break;
                case ModificationPackageBuilderAction.Msix:
                    this.Result = selectedPath;
                    break;
                case ModificationPackageBuilderAction.SignedMsix:

                    switch (this.SelectedCertificate.Store.CurrentValue)
                    {
                        case CertificateSource.Pfx:
                            await this.signingManager.SignPackage(selectedPath, true, this.SelectedCertificate.PfxPath.CurrentValue, this.SelectedCertificate.Password.CurrentValue, this.SelectedCertificate.TimeStamp.CurrentValue, cancellationToken, progress).ConfigureAwait(false);
                            break;
                        case CertificateSource.Personal:
                            await this.signingManager.SignPackage(selectedPath, true, this.SelectedCertificate.SelectedPersonalCertificate?.CurrentValue?.Model, this.SelectedCertificate.TimeStamp.CurrentValue,cancellationToken, progress).ConfigureAwait(false);
                            break;
                    }

                    this.Result = selectedPath;
                    break;
            }

            return true;
        }

        private void ResetExecuted(object parameter)
        {
            this.PackageSelection.Reset();
            this.State.IsSaved = false;
        }

        private void CreateOnValueChanged(object sender, ValueChangedEventArgs e)
        {
            this.SelectedCertificate.IsValidated = this.Create.CurrentValue == ModificationPackageBuilderAction.SignedMsix;
        }

        private void OpenSuccessLinkExecuted(object parameter)
        {
            Process.Start("explorer.exe", "/select," + this.Result);
        }
    }
}

