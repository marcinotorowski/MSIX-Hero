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
        private readonly IInteractionService interactionService;
        private ICommand openSuccessLink;
        private ICommand reset;

        public ModificationPackageViewModel(
            IAppxContentBuilder contentBuilder,
            ISelfElevationProxyProvider<ISigningManager> signingManagerFactory,
            IConfigurationService configurationService,
            IInteractionService interactionService) : base("Create modification package stub", interactionService)
        {
            this.contentBuilder = contentBuilder;
            this.signingManagerFactory = signingManagerFactory;
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
            this.ModificationPackageDetails.Architecture.CurrentValue = (AppxPackageArchitecture)Enum.Parse(typeof(AppxPackageArchitecture), AppInstallerPackageArchitecture.neutral.ToString("G"), true);
            this.ModificationPackageDetails.Commit();
            
            this.Create = new ChangeableProperty<ModificationPackageBuilderAction>();
            this.Create.ValueChanged += this.CreateOnValueChanged;

            this.SelectedCertificate = new CertificateSelectorViewModel(interactionService, signingManagerFactory, configurationService.GetCurrentConfiguration()?.Signing, true);
            
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
                Version = this.ModificationPackageDetails.Version.CurrentValue,
                ParentName = this.PackageSelection.Name.CurrentValue,
                ParentPublisher = this.PackageSelection.Publisher.CurrentValue
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

                    switch (this.SelectedCertificate.Store.CurrentValue)
                    {
                        case CertificateSource.Pfx:
                            await manager.SignPackage(selectedPath, true, this.SelectedCertificate.PfxPath.CurrentValue, this.SelectedCertificate.Password.CurrentValue, this.SelectedCertificate.TimeStamp.CurrentValue, IncreaseVersionMethod.None, cancellationToken, progress).ConfigureAwait(false);
                            break;
                        case CertificateSource.Personal:
                            await manager.SignPackage(selectedPath, true, this.SelectedCertificate.SelectedPersonalCertificate?.CurrentValue?.Model, this.SelectedCertificate.TimeStamp.CurrentValue, IncreaseVersionMethod.None,cancellationToken, progress).ConfigureAwait(false);
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

