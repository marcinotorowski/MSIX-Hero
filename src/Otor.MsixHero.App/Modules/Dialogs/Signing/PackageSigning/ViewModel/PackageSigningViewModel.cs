using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Otor.MsixHero.App.Controls.CertificateSelector.ViewModel;
using Otor.MsixHero.App.Helpers;
using Otor.MsixHero.App.Mvvm.Changeable;
using Otor.MsixHero.App.Mvvm.Changeable.Dialog.ViewModel;
using Otor.MsixHero.Appx.Signing;
using Otor.MsixHero.Appx.Signing.Entities;
using Otor.MsixHero.Cli.Verbs;
using Otor.MsixHero.Infrastructure.Configuration;
using Otor.MsixHero.Infrastructure.Processes.SelfElevation;
using Otor.MsixHero.Infrastructure.Processes.SelfElevation.Enums;
using Otor.MsixHero.Infrastructure.Progress;
using Otor.MsixHero.Infrastructure.Services;
using Prism.Commands;
using Prism.Services.Dialogs;

namespace Otor.MsixHero.App.Modules.Dialogs.Signing.PackageSigning.ViewModel
{
    public class PackageSigningViewModel : ChangeableAutomatedDialogViewModel<SignVerb>, IDialogAware
    {
        private readonly ISelfElevationProxyProvider<ISigningManager> signingManagerFactory;
        private readonly IInteractionService interactionService;
        private readonly IConfigurationService configurationService;
        private ICommand openSuccessLink, reset;

        public PackageSigningViewModel(
            ISelfElevationProxyProvider<ISigningManager> signingManagerFactory, 
            IInteractionService interactionService, 
            IConfigurationService configurationService) : base("Package Signing", interactionService)
        {
            this.signingManagerFactory = signingManagerFactory;
            this.interactionService = interactionService;
            this.configurationService = configurationService;

            this.Files = new ValidatedChangeableCollection<string>(this.ValidateFiles);
            this.IncreaseVersion = new ChangeableProperty<IncreaseVersionMethod>();
            this.CertificateSelector = new CertificateSelectorViewModel(
                interactionService, 
                signingManagerFactory, 
                configurationService?.GetCurrentConfiguration()?.Signing);
            this.OverrideSubject = new ChangeableProperty<bool>(true);

            this.TabPackages = new ChangeableContainer(this.Files);
            this.TabAdjustments = new ChangeableContainer(this.IncreaseVersion);
            this.TabCertificate = new ChangeableContainer(this.CertificateSelector);

            this.AddChildren(this.TabPackages, this.TabCertificate, this.TabAdjustments, this.OverrideSubject);
            this.Files.CollectionChanged += (sender, args) =>
            {
                this.OnPropertyChanged(nameof(IsOnePackage));
            };

            this.RegisterForCommandLineGeneration(
                this.TabCertificate,
                this.TabPackages,
                this.TabAdjustments, 
                this.OverrideSubject);
        }

        protected override void UpdateVerbData()
        {
            this.Verb.FilePath = this.Files;
            this.Verb.IncreaseVersion = this.IncreaseVersion.CurrentValue;
            this.Verb.NoPublisherUpdate = !this.OverrideSubject.CurrentValue;

            var signConfig = this.configurationService.GetCurrentConfiguration().Signing;

            if (this.CertificateSelector.Store.CurrentValue == CertificateSource.DeviceGuard)
            {
                if (signConfig?.Source == CertificateSource.DeviceGuard && 
                    signConfig.DeviceGuard.Subject == this.CertificateSelector.DeviceGuard.CurrentValue.Subject &&
                    signConfig.DeviceGuard.UseV1 == this.CertificateSelector.DeviceGuard.CurrentValue.UseV1 &&
                    signConfig.DeviceGuard.EncodedAccessToken == this.CertificateSelector.DeviceGuard.CurrentValue.EncodedAccessToken &&
                    signConfig.DeviceGuard.EncodedRefreshToken == this.CertificateSelector.DeviceGuard.CurrentValue.EncodedRefreshToken)
                {
                    // do nothing, we have defaults so ideally just no additional command line
                    this.Verb.DeviceGuardInteractive = false;
                    this.Verb.DeviceGuardFile = null;
                    this.Verb.DeviceGuardSubject = null;
                    this.Verb.DeviceGuardVersion1 = false;
                }
                else
                {
                    this.Verb.DeviceGuardInteractive = true;
                    this.Verb.DeviceGuardFile = null;
                    this.Verb.DeviceGuardSubject = this.CertificateSelector.DeviceGuard.CurrentValue?.Subject;
                    this.Verb.DeviceGuardVersion1 = this.CertificateSelector.DeviceGuard.CurrentValue?.UseV1 ?? false;
                }
            }
            else
            {
                this.Verb.DeviceGuardInteractive = false;
                this.Verb.DeviceGuardFile = null;
                this.Verb.DeviceGuardSubject = null;
                this.Verb.DeviceGuardVersion1 = false;
            }

            if (this.CertificateSelector.Store.CurrentValue == CertificateSource.Pfx)
            {
                this.Verb.PfxFilePath = this.CertificateSelector.PfxPath.CurrentValue;
                this.Verb.PfxPassword = this.CertificateSelector.Password.CurrentValue?.Length > 0 ? "<your-password>" : null;

                if (string.IsNullOrEmpty(this.Verb.PfxFilePath))
                {
                    this.Verb.PfxFilePath = "<path-to-pfx-file>";
                }
            }
            else
            {
                this.Verb.PfxFilePath = null;
                this.Verb.PfxPassword = null;
            }

            if (this.CertificateSelector.Store.CurrentValue == CertificateSource.Personal)
            {
                this.Verb.ThumbPrint = this.CertificateSelector.SelectedPersonalCertificate.CurrentValue?.Model.Thumbprint;
                var store = this.CertificateSelector.SelectedPersonalCertificate.CurrentValue?.StoreType;

                switch (store)
                {
                    case CertificateStoreType.Machine:
                    case CertificateStoreType.MachineUser:
                        this.Verb.UseMachineStore = true;
                        break;
                    default:
                        this.Verb.UseMachineStore = false;
                        break;
                }

                if (string.IsNullOrEmpty(this.Verb.ThumbPrint))
                {
                    this.Verb.ThumbPrint = "<thumbprint-to-certificate>";
                }
            }
            else
            {
                this.Verb.ThumbPrint = null;
                this.Verb.UseMachineStore = false;
            }

            this.Verb.NoPublisherUpdate = !this.OverrideSubject.CurrentValue;
            this.Verb.TimeStampUrl = this.CertificateSelector.TimeStamp.CurrentValue;
        }

        public ChangeableContainer TabPackages { get; }

        public ChangeableContainer TabAdjustments { get; }

        public ChangeableContainer TabCertificate { get; }

        public CertificateSelectorViewModel CertificateSelector { get; }

        public ChangeableProperty<IncreaseVersionMethod> IncreaseVersion { get; }

        public ChangeableProperty<bool> OverrideSubject { get; }

        public List<string> SelectedPackages { get; } = new List<string>();

        void IDialogAware.OnDialogOpened(IDialogParameters parameters)
        {
            if (parameters.TryGetValue<string>("Path", out var file))
            {
                this.Files.Add(file);
            }
            else
            {
                var interactionResult = this.interactionService.SelectFiles(filterString: "MSIX packages (*.msix)|*.msix", out string[] selection);
                if (!interactionResult || !selection.Any())
                {
                    return;
                }

                foreach (var selected in selection)
                {
                    if (this.Files.Contains(selected))
                    {
                        continue;
                    }

                    this.Files.Add(selected);
                }
            }

            this.Files.Commit();
        }

        protected override async Task<bool> Save(CancellationToken cancellationToken, IProgress<ProgressData> progress)
        {
            var manager = await this.signingManagerFactory.GetProxyFor(SelfElevationLevel.AsInvoker, cancellationToken).ConfigureAwait(false);
            cancellationToken.ThrowIfCancellationRequested();

            foreach (var file in this.Files)
            {
                cancellationToken.ThrowIfCancellationRequested();

                switch (this.CertificateSelector.Store.CurrentValue)
                {
                    case CertificateSource.Pfx:
                        await manager.SignPackageWithPfx(file, this.OverrideSubject.CurrentValue, this.CertificateSelector.PfxPath.CurrentValue, this.CertificateSelector.Password.CurrentValue, this.CertificateSelector.TimeStamp.CurrentValue, this.IncreaseVersion.CurrentValue, cancellationToken, progress).ConfigureAwait(false);
                        break;
                    case CertificateSource.Personal:
                        await manager.SignPackageWithInstalled(file, this.OverrideSubject.CurrentValue, this.CertificateSelector.SelectedPersonalCertificate.CurrentValue.Model, this.CertificateSelector.TimeStamp.CurrentValue, this.IncreaseVersion.CurrentValue, cancellationToken, progress).ConfigureAwait(false);
                        break;
                    case CertificateSource.DeviceGuard:
                        await manager.SignPackageWithDeviceGuardFromUi(file,  this.CertificateSelector.DeviceGuard.CurrentValue, this.CertificateSelector.TimeStamp.CurrentValue, this.IncreaseVersion.CurrentValue, cancellationToken, progress).ConfigureAwait(false);
                        break;
                }
            }

            return true;
        }

        public ValidatedChangeableCollection<string> Files { get; }

        public ICommand OpenSuccessLinkCommand
        {
            get { return this.openSuccessLink ??= new DelegateCommand(this.OpenSuccessLinkExecuted, this.CanOpenSuccessLinkExecute); }
        }

        public bool IsOnePackage
        {
            get => this.Files.Count == 1;
        }

        public ICommand ResetCommand
        {
            get { return this.reset ??= new DelegateCommand(this.ResetExecuted); }
        }

        private string ValidateFiles(IEnumerable<string> files)
        {
            if (!files.Any())
            {
                return "At least one file is required.";
            }

            return null;
        }

        private void ResetExecuted()
        {
            this.Files.Clear();
            this.Commit();
            this.State.IsSaved = false;
        }

        private void OpenSuccessLinkExecuted()
        {
            if (!this.IsOnePackage)
            {
                return;
            }

            Process.Start("explorer.exe", "/select," + this.Files[0]);
        }

        private bool CanOpenSuccessLinkExecute()
        {
            return this.IsOnePackage;
        }
    }
}

