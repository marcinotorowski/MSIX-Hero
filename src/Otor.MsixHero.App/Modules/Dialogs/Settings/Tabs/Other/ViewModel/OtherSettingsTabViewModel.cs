using System.Threading.Tasks;
using Otor.MsixHero.App.Modules.Dialogs.Settings.Context;
using Otor.MsixHero.App.Mvvm.Changeable;
using Otor.MsixHero.Infrastructure.Configuration;
using Otor.MsixHero.Infrastructure.Logging;
using Otor.MsixHero.Infrastructure.Services;
using Prism.Services.Dialogs;

namespace Otor.MsixHero.App.Modules.Dialogs.Settings.Tabs.Other.ViewModel
{
    public class OtherSettingsTabViewModel : ChangeableContainer, ISettingsComponent
    {
        public OtherSettingsTabViewModel(IConfigurationService configurationService, IInteractionService interactionService)
        {
            var config = configurationService.GetCurrentConfiguration();

            this.AddChildren(
                this.CertificateOutputPath = new ChangeableFolderProperty(() => Resources.Localization.Dialogs_Settings_Certificate_Output, interactionService, config.Signing?.DefaultOutFolder?.Resolved, ChangeableFolderProperty.ValidatePath),
                this.PackerSignByDefault = new ChangeableProperty<bool>(config.Packer?.SignByDefault == true),
                this.DefaultRemoteLocationPackages = new ValidatedChangeableProperty<string>(() => Resources.Localization.Dialogs_Settings_AppInstaller_RemoteMsix, config.AppInstaller?.DefaultRemoteLocationPackages, ValidatorFactory.ValidateUri(false)),
                this.DefaultRemoteLocationAppInstaller = new ValidatedChangeableProperty<string>(() => Resources.Localization.Dialogs_Settings_AppInstaller_RemoteUrl, config.AppInstaller?.DefaultRemoteLocationAppInstaller, ValidatorFactory.ValidateUri(false)),
                this.VerboseLogging = new ChangeableProperty<bool>(config.VerboseLogging)
            );
        }

        public ChangeableProperty<bool> VerboseLogging { get; }

        public ChangeableProperty<bool> PackerSignByDefault { get; }

        public ChangeableFolderProperty CertificateOutputPath { get; }

        public ValidatedChangeableProperty<string> DefaultRemoteLocationPackages { get; }

        public ValidatedChangeableProperty<string> DefaultRemoteLocationAppInstaller { get; }

        public void Register(ISettingsContext context)
        {
            context.Register(this);
        }

        public bool CanCloseDialog() => true;

        public void OnDialogOpened(IDialogParameters parameters)
        {
        }

        public void OnDialogClosed()
        {
        }

        public bool CanSave() => !this.IsTouched || !this.IsValidated || this.IsValid;

        public Task<bool> OnSaving(Configuration newConfiguration)
        {
            if (this.CertificateOutputPath.IsTouched)
            {
                newConfiguration.Signing.DefaultOutFolder = this.CertificateOutputPath.CurrentValue;
            }

            if (this.PackerSignByDefault.IsTouched)
            {
                newConfiguration.Packer.SignByDefault = this.PackerSignByDefault.CurrentValue;
            }

            if (this.DefaultRemoteLocationAppInstaller.IsTouched)
            {
                newConfiguration.AppInstaller.DefaultRemoteLocationAppInstaller = this.DefaultRemoteLocationAppInstaller.CurrentValue;
            }

            if (this.DefaultRemoteLocationPackages.IsTouched)
            {
                newConfiguration.AppInstaller.DefaultRemoteLocationPackages = this.DefaultRemoteLocationPackages.CurrentValue;
            }

            if (this.VerboseLogging.IsTouched)
            {
                newConfiguration.VerboseLogging = this.VerboseLogging.CurrentValue;
                LogManager.Initialize(newConfiguration.VerboseLogging ? MsixHeroLogLevel.Trace : MsixHeroLogLevel.Info);
            }

            return Task.FromResult(true);
        }
    }
}
