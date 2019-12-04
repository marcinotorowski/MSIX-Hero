using System;
using System.Threading.Tasks;
using otor.msixhero.lib.Infrastructure;
using otor.msixhero.lib.Infrastructure.Configuration;
using otor.msixhero.ui.Domain;
using Prism.Services.Dialogs;

namespace otor.msixhero.ui.Modules.Settings.ViewModel
{
    public class SettingsViewModel : IDialogAware
    {
        private readonly IConfigurationService configurationService;

        public SettingsViewModel(IConfigurationService configurationService, IInteractionService interactionService)
        {
            this.configurationService = configurationService;

            var config = configurationService.GetCurrentConfiguration();

            this.AllSettings.AddChildren(
                this.CertificateOutputPath = new ChangeableFolderProperty(interactionService, config.Signing?.DefaultOutFolder?.Resolved),
                this.TimeStampServer = new ChangeableProperty<string>(config.Signing?.TimeStampServer),
                this.SidebarDefaultState = new ChangeableProperty<bool>(config.List.Sidebar.Visible)
            );

            this.CertificateOutputPath.Validator = ChangeableFolderProperty.ValidatePath;
        }

        public ChangeableContainer AllSettings { get; } = new ChangeableContainer();

        public ChangeableFolderProperty CertificateOutputPath { get; }

        public ChangeableProperty<string> TimeStampServer { get; }
        
        public ChangeableProperty<bool> SidebarDefaultState { get; }

        public string Title => "Settings";

        public bool CanCloseDialog()
        {
            return true;
        }

        public void OnDialogClosed()
        {
        }

        public void OnDialogOpened(IDialogParameters parameters)
        {
        }

        public bool CanSave()
        {
            return this.AllSettings.IsTouched && (!this.AllSettings.IsValidated || this.AllSettings.IsValid);
        }

        public async Task<bool> Save()
        {
            if (!this.AllSettings.IsValidated)
            {
                this.AllSettings.IsValidated = true;
            }

            if (!this.AllSettings.IsValid)
            {
                return false;
            }

            var newConfiguration = this.configurationService.GetCurrentConfiguration(false);

            if (this.CertificateOutputPath.IsTouched)
            {
                newConfiguration.Signing.DefaultOutFolder = this.CertificateOutputPath.CurrentValue;
            }

            if (this.TimeStampServer.IsTouched)
            {
                newConfiguration.Signing.TimeStampServer = this.TimeStampServer.CurrentValue;
            }
            
            if (this.SidebarDefaultState.IsTouched)
            {
                newConfiguration.List.Sidebar.Visible = this.SidebarDefaultState.CurrentValue;
            }
            
            this.AllSettings.Commit();
            await this.configurationService.SetCurrentConfigurationAsync(newConfiguration);
            return true;
        }

        public event Action<IDialogResult> RequestClose;
    }
}
