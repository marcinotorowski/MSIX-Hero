using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using otor.msixhero.lib.BusinessLayer.Appx.Signing;
using otor.msixhero.lib.Infrastructure;
using otor.msixhero.lib.Infrastructure.Configuration;
using otor.msixhero.lib.Infrastructure.Progress;
using otor.msixhero.ui.Domain;
using otor.msixhero.ui.Modules.Dialogs.Common.CertificateSelector.ViewModel;
using Prism.Services.Dialogs;

namespace otor.msixhero.ui.Modules.Dialogs.PackageSigning.ViewModel
{
    public class PackageSigningViewModel : ChangeableContainer, IDialogAware
    {
        private readonly IAppxSigningManager signingManager;
        private readonly IInteractionService interactionService;
        private int progress;
        private string progressMessage;
        private bool isLoading;
        private bool isSuccess;

        public PackageSigningViewModel(IAppxSigningManager signingManager, IInteractionService interactionService, IConfigurationService configurationService)
        {
            this.signingManager = signingManager;
            this.interactionService = interactionService;
            this.Files = new ValidatedChangeableCollection<string>(this.ValidateFiles);
            this.SelectedCertificate = new CertificateSelectorViewModel(interactionService, signingManager, configurationService?.GetCurrentConfiguration()?.Signing, true);

            this.AddChildren(this.Files, this.SelectedCertificate);
            this.IsValidated = false;
        }
        
        public CertificateSelectorViewModel SelectedCertificate { get; }

        public bool IsLoading
        {
            get => this.isLoading;
            set => this.SetField(ref this.isLoading, value);
        }

        public int Progress
        {
            get => this.progress;
            private set => this.SetField(ref this.progress, value);
        }

        public string ProgressMessage
        {
            get => this.progressMessage;
            private set => this.SetField(ref this.progressMessage, value);
        }

        public bool CanCloseDialog()
        {
            return true;
        }
        
        public void OnDialogClosed()
        {
        }
        
        public void OnDialogOpened(IDialogParameters parameters)
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

        public async Task Save()
        {
            this.IsValidated = true;
            if (!this.IsValid)
            {
                return;
            }

            var token = new Progress();

            EventHandler<ProgressData> handler = (sender, data) =>
            {
                this.Progress = data.Progress;
                this.ProgressMessage = data.Message;
            };

            this.IsLoading = true;
            try
            {
                token.ProgressChanged += handler;
                // TODO

                foreach (var file in this.Files)
                {
                    if (this.SelectedCertificate.Store.CurrentValue == CertificateSource.Pfx)
                    {
                        await this.signingManager.SignPackage(file, true, this.SelectedCertificate.PfxPath.CurrentValue, this.SelectedCertificate.Password.CurrentValue, this.SelectedCertificate.TimeStamp.CurrentValue, CancellationToken.None, token);
                    }
                    else
                    {
                        await this.signingManager.SignPackage(file, true, this.SelectedCertificate.SelectedPersonalCertificate.CurrentValue.Model, this.SelectedCertificate.TimeStamp.CurrentValue, CancellationToken.None, token);
                    }
                }

                this.IsSuccess = true;
            }
            finally
            {
                token.ProgressChanged -= handler;
                this.IsLoading = false;
                this.Progress = 100;
                this.ProgressMessage = null;
            }
        }

        public ValidatedChangeableCollection<string> Files { get; }

        public bool IsSuccess
        {
            get => this.isSuccess;
            set => this.SetField(ref this.isSuccess, value);
        }
        
        private string ValidateFiles(IEnumerable<string> files)
        {
            if (!files.Any())
            {
                return "At least one file is required.";
            }

            return null;
        }

        public bool CanSave()
        {
            return this.IsTouched && this.IsValid;
        }

        public string Title
        {
            get => "Package Signing";
        }

        public event Action<IDialogResult> RequestClose;
    }
}

