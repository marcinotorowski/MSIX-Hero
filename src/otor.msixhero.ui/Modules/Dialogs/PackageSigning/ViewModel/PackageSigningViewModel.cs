using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using otor.msixhero.lib.BusinessLayer.Appx.Signing;
using otor.msixhero.lib.Infrastructure;
using otor.msixhero.lib.Infrastructure.Configuration;
using otor.msixhero.lib.Infrastructure.Progress;
using otor.msixhero.ui.Commands.RoutedCommand;
using otor.msixhero.ui.Controls.ChangeableDialog.ViewModel;
using otor.msixhero.ui.Domain;
using otor.msixhero.ui.Modules.Dialogs.Common.CertificateSelector.ViewModel;
using Prism.Services.Dialogs;

namespace otor.msixhero.ui.Modules.Dialogs.PackageSigning.ViewModel
{
    public class PackageSigningViewModel : ChangeableDialogViewModel, IDialogAware
    {
        private readonly IAppxSigningManager signingManager;
        private readonly IInteractionService interactionService;
        private ICommand openSuccessLink, reset;

        public PackageSigningViewModel(IAppxSigningManager signingManager, IInteractionService interactionService, IConfigurationService configurationService) : base("Package Signing", interactionService)
        {
            this.signingManager = signingManager;
            this.interactionService = interactionService;
            this.Files = new ValidatedChangeableCollection<string>(this.ValidateFiles);
            this.SelectedCertificate = new CertificateSelectorViewModel(interactionService, signingManager, configurationService?.GetCurrentConfiguration()?.Signing, true);

            this.AddChildren(this.Files, this.SelectedCertificate);
            this.SetValidationMode(ValidationMode.Silent, true);
            this.Files.CollectionChanged += (sender, args) => { this.OnPropertyChanged(nameof(IsOnePackage)); };
        }
        
        public CertificateSelectorViewModel SelectedCertificate { get; }

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
            foreach (var file in this.Files)
            {
                if (this.SelectedCertificate.Store.CurrentValue == CertificateSource.Pfx)
                {
                    await this.signingManager.SignPackage(file, true, this.SelectedCertificate.PfxPath.CurrentValue, this.SelectedCertificate.Password.CurrentValue, this.SelectedCertificate.TimeStamp.CurrentValue, cancellationToken, progress);
                }
                else
                {
                    await this.signingManager.SignPackage(file, true, this.SelectedCertificate.SelectedPersonalCertificate.CurrentValue.Model, this.SelectedCertificate.TimeStamp.CurrentValue, cancellationToken, progress);
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

