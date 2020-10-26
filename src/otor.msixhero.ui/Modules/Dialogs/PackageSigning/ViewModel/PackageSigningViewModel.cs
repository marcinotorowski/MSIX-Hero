using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
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
using Prism.Services.Dialogs;

namespace Otor.MsixHero.Ui.Modules.Dialogs.PackageSigning.ViewModel
{
    public class PackageSigningViewModel : ChangeableDialogViewModel, IDialogAware
    {
        private readonly ISelfElevationProxyProvider<ISigningManager> signingManagerFactory;
        private readonly IInteractionService interactionService;
        private ICommand openSuccessLink, reset;

        public PackageSigningViewModel(ISelfElevationProxyProvider<ISigningManager> signingManagerFactory, IInteractionService interactionService, IConfigurationService configurationService) : base("Package Signing", interactionService)
        {
            this.signingManagerFactory = signingManagerFactory;
            this.interactionService = interactionService;
            
            this.Files = new ValidatedChangeableCollection<string>(this.ValidateFiles);
            this.IncreaseVersion = new ChangeableProperty<IncreaseVersionMethod>();
            
            this.TabCertificate = new CertificateSelectorViewModel(interactionService, signingManagerFactory, configurationService?.GetCurrentConfiguration()?.Signing, true);
            this.TabPackages = new ChangeableContainer(this.Files);
            this.TabAdjustments = new ChangeableContainer(this.IncreaseVersion);

            this.AddChildren(this.TabPackages, this.TabCertificate, this.TabAdjustments);
            this.Files.CollectionChanged += (sender, args) =>
            {
                this.OnPropertyChanged(nameof(IsOnePackage));
            };
        }
        
        public ChangeableContainer TabPackages { get; }

        public ChangeableContainer TabAdjustments { get; }

        public CertificateSelectorViewModel TabCertificate { get; }

        public ChangeableProperty<IncreaseVersionMethod> IncreaseVersion { get; }

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

                switch (this.TabCertificate.Store.CurrentValue)
                {
                    case CertificateSource.Pfx:
                        await manager.SignPackageWithPfx(file, true, this.TabCertificate.PfxPath.CurrentValue, this.TabCertificate.Password.CurrentValue, this.TabCertificate.TimeStamp.CurrentValue, this.IncreaseVersion.CurrentValue, cancellationToken, progress).ConfigureAwait(false);
                        break;
                    case CertificateSource.Personal:
                        await manager.SignPackageWithInstalled(file, true, this.TabCertificate.SelectedPersonalCertificate.CurrentValue.Model, this.TabCertificate.TimeStamp.CurrentValue, this.IncreaseVersion.CurrentValue, cancellationToken, progress).ConfigureAwait(false);
                        break;
                    case CertificateSource.DeviceGuard:
                        await manager.SignPackageWithDeviceGuard(file, Guid.Parse(this.TabCertificate.ClientId.CurrentValue), this.TabCertificate.Secret.CurrentValue, this.TabCertificate.TimeStamp.CurrentValue, this.IncreaseVersion.CurrentValue, cancellationToken, progress).ConfigureAwait(false);
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

        private void ResetExecuted(object parameter)
        {
            this.Files.Clear();
            this.Commit();
            this.State.IsSaved = false;
        }

        private void OpenSuccessLinkExecuted(object parameter)
        {
            if (!this.IsOnePackage)
            {
                return;
            }

            Process.Start("explorer.exe", "/select," + this.Files[0]);
        }

        private bool CanOpenSuccessLinkExecute(object parameter)
        {
            return this.IsOnePackage;
        }
    }
}

