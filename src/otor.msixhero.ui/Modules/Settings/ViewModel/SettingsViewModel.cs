using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Otor.MsixHero.Appx.Signing;
using Otor.MsixHero.Infrastructure.Configuration;
using Otor.MsixHero.Infrastructure.Cryptography;
using Otor.MsixHero.Infrastructure.Processes.SelfElevation;
using Otor.MsixHero.Infrastructure.Services;
using Otor.MsixHero.Ui.Domain;
using Otor.MsixHero.Ui.Hero.Events;
using Otor.MsixHero.Ui.Modules.Dialogs.Common.CertificateSelector.ViewModel;
using Otor.MsixHero.Ui.Modules.Settings.ViewModel.Tools;
using Otor.MsixHero.Ui.ViewModel;
using Prism.Events;
using Prism.Services.Dialogs;

namespace Otor.MsixHero.Ui.Modules.Settings.ViewModel
{
    public class SettingsViewModel : NotifyPropertyChanged, IDialogAware
    {
        private readonly IEventAggregator eventAggregator;
        private readonly IConfigurationService configurationService;
        private string entryPoint;

        public SettingsViewModel(
            IEventAggregator eventAggregator,
            IConfigurationService configurationService, 
            IInteractionService interactionService, 
            ISelfElevationProxyProvider<ISigningManager> signingManagerFactory)
        {
            this.eventAggregator = eventAggregator;
            this.configurationService = configurationService;

            var config = configurationService.GetCurrentConfiguration() ?? new Configuration();
            
            this.AllSettings.AddChildren(
                this.CertificateOutputPath = new ChangeableFolderProperty("Certificate output path", interactionService, config.Signing?.DefaultOutFolder?.Resolved),
                this.PackerSignByDefault = new ChangeableProperty<bool>(config.Packer?.SignByDefault == true),
                this.SidebarDefaultState = new ChangeableProperty<bool>(config.List.Sidebar.Visible),
                this.SwitchToContextualTabAfterSelection = new ChangeableProperty<bool>(config.UiConfiguration.SwitchToContextTabAfterSelection),
                this.ConfirmDeletion = new ChangeableProperty<bool>(config.UiConfiguration.ConfirmDeletion),
                this.CertificateSelector = new CertificateSelectorViewModel(interactionService, signingManagerFactory, config.Signing, true),
                this.ManifestEditorType = new ChangeableProperty<EditorType>(config.Editing.ManifestEditorType),
                this.ManifestEditorPath = new ChangeableFileProperty("Manifest editor path", interactionService, config.Editing.ManifestEditor.Resolved),
                this.MsixEditorType = new ChangeableProperty<EditorType>(config.Editing.ManifestEditorType),
                this.MsixEditorPath = new ChangeableFileProperty("MSIX editor path", interactionService, config.Editing.MsixEditor.Resolved),
                this.AppinstallerEditorType = new ChangeableProperty<EditorType>(config.Editing.AppInstallerEditorType),
                this.AppinstallerEditorPath = new ChangeableFileProperty("App installer editor path", interactionService, config.Editing.AppInstallerEditor.Resolved),
                this.PsfEditorType = new ChangeableProperty<EditorType>(config.Editing.PsfEditorType),
                this.PsfEditorPath = new ChangeableFileProperty("PSF editor path", interactionService, config.Editing.PsfEditor.Resolved),
                this.PowerShellEditorType = new ChangeableProperty<EditorType>(config.Editing.PowerShellEditorType),
                this.PowerShellEditorPath = new ChangeableFileProperty("PowerShell editor path", interactionService, config.Editing.PowerShellEditor.Resolved),
                this.DefaultRemoteLocationPackages = new ValidatedChangeableProperty<string>("Remote .msix URL", config.AppInstaller?.DefaultRemoteLocationPackages, this.ValidateUri),
                this.DefaultRemoteLocationAppInstaller = new ValidatedChangeableProperty<string>("Remote .appinstaller URL", config.AppInstaller?.DefaultRemoteLocationAppInstaller, this.ValidateUri),
                this.Tools = new ToolsConfigurationViewModel(interactionService, config)
            );

            this.CertificateOutputPath.Validators = new[] { ChangeableFolderProperty.ValidatePath };
        }
        
        public ToolsConfigurationViewModel Tools { get; }

        public CertificateSelectorViewModel CertificateSelector { get; }

        public ChangeableContainer AllSettings { get; } = new ChangeableContainer();

        public ChangeableFolderProperty CertificateOutputPath { get; }
        
        public ValidatedChangeableProperty<string> DefaultRemoteLocationPackages { get; }

        public ValidatedChangeableProperty<string> DefaultRemoteLocationAppInstaller { get; }

        public ChangeableProperty<EditorType> ManifestEditorType { get; }

        public ChangeableFileProperty ManifestEditorPath { get; }

        public ChangeableProperty<EditorType> MsixEditorType { get; }

        public ChangeableFileProperty MsixEditorPath { get; }

        public ChangeableProperty<EditorType> AppinstallerEditorType { get; }

        public ChangeableFileProperty AppinstallerEditorPath { get; }

        public ChangeableProperty<EditorType> PsfEditorType { get; }

        public ChangeableFileProperty PsfEditorPath { get; }

        public ChangeableProperty<EditorType> PowerShellEditorType { get; }

        public ChangeableFileProperty PowerShellEditorPath { get; }

        public ChangeableProperty<bool> PackerSignByDefault { get; }
        
        public ChangeableProperty<bool> SidebarDefaultState { get; }

        public ChangeableProperty<bool> SwitchToContextualTabAfterSelection { get; }

        public ChangeableProperty<bool> ConfirmDeletion { get; }

        public string EntryPoint
        {
            get => this.entryPoint;
            private set => this.SetField(ref this.entryPoint, value);
        }

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
            if (!parameters.TryGetValue("tab", out string tab))
            {
                return;
            }

            this.EntryPoint = tab;
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

            if (this.PackerSignByDefault.IsTouched)
            {
                newConfiguration.Packer.SignByDefault = this.PackerSignByDefault.CurrentValue;
            }
            
            if (this.SidebarDefaultState.IsTouched)
            {
                newConfiguration.List.Sidebar.Visible = this.SidebarDefaultState.CurrentValue;
            }

            if (this.SwitchToContextualTabAfterSelection.IsTouched)
            {
                newConfiguration.UiConfiguration.SwitchToContextTabAfterSelection = this.SwitchToContextualTabAfterSelection.CurrentValue;
            }

            if (this.ConfirmDeletion.IsTouched)
            {
                newConfiguration.UiConfiguration.ConfirmDeletion = this.ConfirmDeletion.CurrentValue;
            }

            if (this.CertificateSelector.IsTouched)
            {
                if (this.CertificateSelector.Store.IsTouched || this.CertificateSelector.PfxPath.IsTouched)
                {
                    newConfiguration.Signing.PfxPath.Resolved = this.CertificateSelector.PfxPath.CurrentValue;
                }

                if (this.CertificateSelector.Store.IsTouched || this.CertificateSelector.Password.IsTouched)
                {
                    if (this.CertificateSelector.Password?.CurrentValue == null || this.CertificateSelector.Password.CurrentValue.Length == 0)
                    {
                        newConfiguration.Signing.EncodedPassword = null;
                    }
                    else
                    {
                        var valuePtr = IntPtr.Zero;
                        try
                        {
                            valuePtr = Marshal.SecureStringToGlobalAllocUnicode(this.CertificateSelector.Password.CurrentValue);

                            var encoder = new Crypto();
                            // newConfiguration.Signing.EncodedPassword = encoder.EncryptString(Marshal.PtrToStringUni(valuePtr), "$%!!ASddahs55839AA___ąółęńśSdcvv");
                            newConfiguration.Signing.EncodedPassword = encoder.Protect(Marshal.PtrToStringUni(valuePtr));
                        }
                        finally
                        {
                            Marshal.ZeroFreeGlobalAllocUnicode(valuePtr);
                        }
                    }
                }

                if (this.CertificateSelector.Store.IsTouched || this.CertificateSelector.SelectedPersonalCertificate.IsTouched)
                {
                    newConfiguration.Signing.Thumbprint = this.CertificateSelector.SelectedPersonalCertificate.CurrentValue?.Model?.Thumbprint;
                }

                if (this.CertificateSelector.Store.IsTouched)
                {
                    newConfiguration.Signing.Source = this.CertificateSelector.Store.CurrentValue;
                }

                if (this.CertificateSelector.TimeStamp.IsTouched)
                {
                    newConfiguration.Signing.TimeStampServer = this.CertificateSelector.TimeStamp.CurrentValue;
                }
            }

            if (this.ManifestEditorType.IsTouched)
            {
                newConfiguration.Editing.ManifestEditorType = this.ManifestEditorType.CurrentValue;
            }

            if (this.AppinstallerEditorType.IsTouched)
            {
                newConfiguration.Editing.AppInstallerEditorType = this.AppinstallerEditorType.CurrentValue;
            }

            if (this.MsixEditorType.IsTouched)
            {
                newConfiguration.Editing.MsixEditorType = this.MsixEditorType.CurrentValue;
            }

            if (this.PsfEditorType.IsTouched)
            {
                newConfiguration.Editing.PsfEditorType = this.PsfEditorType.CurrentValue;
            }

            if (this.PowerShellEditorType.IsTouched)
            {
                newConfiguration.Editing.PowerShellEditorType = this.PowerShellEditorType.CurrentValue;
            }

            if (this.ManifestEditorPath.IsTouched)
            {
                newConfiguration.Editing.ManifestEditor.Resolved = this.ManifestEditorPath.CurrentValue;
            }

            if (this.AppinstallerEditorPath.IsTouched)
            {
                newConfiguration.Editing.AppInstallerEditor.Resolved = this.AppinstallerEditorPath.CurrentValue;
            }

            if (this.MsixEditorPath.IsTouched)
            {
                newConfiguration.Editing.MsixEditor.Resolved = this.MsixEditorPath.CurrentValue;
            }

            if (this.PsfEditorPath.IsTouched)
            {
                newConfiguration.Editing.PsfEditor.Resolved = this.PsfEditorPath.CurrentValue;
            }

            if (this.PowerShellEditorPath.IsTouched)
            {
                newConfiguration.Editing.PowerShellEditor.Resolved = this.PowerShellEditorPath.CurrentValue;
            }

            var toolsTouched = this.Tools.IsTouched;
            this.AllSettings.Commit();

            if (toolsTouched)
            {
                newConfiguration.List.Tools.Clear();
                newConfiguration.List.Tools.AddRange(this.Tools.Items.Select(t => (ToolListConfiguration)t));
            }

            await this.configurationService.SetCurrentConfigurationAsync(newConfiguration).ConfigureAwait(false);

            if (toolsTouched)
            {
                this.eventAggregator.GetEvent<ToolsChangedEvent>().Publish(this.Tools.Items.Select(t => (ToolListConfiguration)t).ToArray());
            }

            return true;
        }

        public event Action<IDialogResult> RequestClose;

        private string ValidateUri(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return null;
            }

            if (!Uri.TryCreate(value, UriKind.Absolute, out _))
            {
                return $"The value '{value}' is not a valid URI.";
            }

            return null;
        }
    }
}
