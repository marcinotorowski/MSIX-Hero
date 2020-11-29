using System;
using System.Linq;
using System.Threading.Tasks;
using Otor.MsixHero.App.Controls.CertificateSelector.ViewModel;
using Otor.MsixHero.App.Hero.Events;
using Otor.MsixHero.App.Modules.Dialogs.Settings.ViewModel.Tools;
using Otor.MsixHero.App.Mvvm;
using Otor.MsixHero.App.Mvvm.Changeable;
using Otor.MsixHero.Appx.Signing;
using Otor.MsixHero.Infrastructure.Configuration;
using Otor.MsixHero.Infrastructure.Cryptography;
using Otor.MsixHero.Infrastructure.Processes.SelfElevation;
using Otor.MsixHero.Infrastructure.Services;
using Prism.Events;
using Prism.Services.Dialogs;

namespace Otor.MsixHero.App.Modules.Dialogs.Settings.ViewModel
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

            this.TabOther.AddChildren
            (
                this.CertificateOutputPath = new ChangeableFolderProperty("Certificate output path", interactionService, config.Signing?.DefaultOutFolder?.Resolved),
                this.PackerSignByDefault = new ChangeableProperty<bool>(config.Packer?.SignByDefault == true),
                this.DefaultRemoteLocationPackages = new ValidatedChangeableProperty<string>("Remote .msix URL", config.AppInstaller?.DefaultRemoteLocationPackages, this.ValidateUri),
                this.DefaultRemoteLocationAppInstaller = new ValidatedChangeableProperty<string>("Remote .appinstaller URL", config.AppInstaller?.DefaultRemoteLocationAppInstaller, this.ValidateUri)
            );

            this.TabEditors.AddChildren
            (
                this.ManifestEditorType = new ChangeableProperty<EditorType>(config.Editing.ManifestEditorType),
                this.ManifestEditorPath = new ChangeableFileProperty("Manifest editor path", interactionService, config.Editing.ManifestEditor.Resolved, this.ValidateManifestEditorPath),
                this.MsixEditorType = new ChangeableProperty<EditorType>(config.Editing.MsixEditorType),
                this.MsixEditorPath = new ChangeableFileProperty("MSIX editor path", interactionService, config.Editing.MsixEditor.Resolved, this.ValidateMsixEditorPath),
                this.AppinstallerEditorType = new ChangeableProperty<EditorType>(config.Editing.AppInstallerEditorType),
                this.AppinstallerEditorPath = new ChangeableFileProperty("App installer editor path", interactionService, config.Editing.AppInstallerEditor.Resolved, this.ValidateAppInstallerEditorPath),
                this.PsfEditorType = new ChangeableProperty<EditorType>(config.Editing.PsfEditorType),
                this.PsfEditorPath = new ChangeableFileProperty("PSF editor path", interactionService, config.Editing.PsfEditor.Resolved, this.ValidatePsfEditorPath),
                this.PowerShellEditorType = new ChangeableProperty<EditorType>(config.Editing.PowerShellEditorType),
                this.PowerShellEditorPath = new ChangeableFileProperty("PowerShell editor path", interactionService, config.Editing.PowerShellEditor.Resolved, this.ValidatePowerShellEditorPath)
            );

            this.TabSigning.AddChildren
            (
                this.CertificateSelector = new CertificateSelectorViewModel(interactionService, signingManagerFactory, config.Signing, true)
            );

            this.AllSettings.AddChildren(
                this.TabSigning,
                this.SidebarDefaultState = new ChangeableProperty<bool>(config.Packages.Sidebar.Visible),
                this.SwitchToContextualTabAfterSelection = new ChangeableProperty<bool>(config.UiConfiguration.SwitchToContextTabAfterSelection),
                this.ConfirmDeletion = new ChangeableProperty<bool>(config.UiConfiguration.ConfirmDeletion),
                this.TabEditors,
                this.Tools = new ToolsConfigurationViewModel(interactionService, config),
                this.TabOther
            );

            this.CertificateOutputPath.Validators = new[] { ChangeableFolderProperty.ValidatePath };

            this.AppinstallerEditorType.ValueChanged += this.TypeOfPathChanged;
            this.ManifestEditorType.ValueChanged += this.TypeOfPathChanged;
            this.PsfEditorType.ValueChanged += this.TypeOfPathChanged;
            this.MsixEditorType.ValueChanged += this.TypeOfPathChanged;
            this.PowerShellEditorType.ValueChanged += this.TypeOfPathChanged;
        }

        private void TypeOfPathChanged(object sender, ValueChangedEventArgs e)
        {
            ChangeableFileProperty changeable;

            if (sender == this.AppinstallerEditorType)
            {
                changeable = this.AppinstallerEditorPath;
            }
            else if (sender == this.MsixEditorType)
            {
                changeable = this.MsixEditorPath;
            }
            else if (sender == this.PsfEditorType)
            {
                changeable = this.PsfEditorPath;
            }
            else if (sender == this.PowerShellEditorType)
            {
                changeable = this.PowerShellEditorPath;
            }
            else if (sender == this.ManifestEditorType)
            {
                changeable = this.ManifestEditorPath;
            }
            else
            {
                return;
            }

            if ((EditorType)e.NewValue == EditorType.Custom)
            {
                changeable.Browse.Execute(null);
                // changeable.CurrentValue = "<custom-path>";
            }
        }

        public string ValidatePowerShellEditorPath(string value)
        {
            if (this.PowerShellEditorType.CurrentValue != EditorType.Custom)
            {
                return null;
            }

            return string.IsNullOrEmpty(value) ? "The path may not be empty." : null;
        }

        public string ValidateManifestEditorPath(string value)
        {
            if (this.ManifestEditorType.CurrentValue != EditorType.Custom)
            {
                return null;
            }

            return string.IsNullOrEmpty(value) ? "The path may not be empty." : null;
        }

        public string ValidateAppInstallerEditorPath(string value)
        {
            if (this.AppinstallerEditorType.CurrentValue != EditorType.Custom)
            {
                return null;
            }

            return string.IsNullOrEmpty(value) ? "The path may not be empty." : null;
        }

        public string ValidateMsixEditorPath(string value)
        {
            if (this.MsixEditorType.CurrentValue != EditorType.Custom)
            {
                return null;
            }

            return string.IsNullOrEmpty(value) ? "The path may not be empty." : null;
        }

        public string ValidatePsfEditorPath(string value)
        {
            if (this.PsfEditorType.CurrentValue != EditorType.Custom)
            {
                return null;
            }

            return string.IsNullOrEmpty(value) ? "The path may not be empty." : null;
        }

        public ChangeableContainer TabOther { get; } = new ChangeableContainer();

        public ChangeableContainer TabEditors { get; } = new ChangeableContainer();

        public ChangeableContainer TabSigning { get; } = new ChangeableContainer();

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

            var newConfiguration = await this.configurationService.GetCurrentConfigurationAsync(false).ConfigureAwait(false);

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
                newConfiguration.Packages.Sidebar.Visible = this.SidebarDefaultState.CurrentValue;
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
                if (this.CertificateSelector.Store.CurrentValue == CertificateSource.Pfx)
                {
                    if (this.CertificateSelector.Store.IsTouched || this.CertificateSelector.PfxPath.IsTouched)
                    {
                        newConfiguration.Signing.PfxPath.Resolved = this.CertificateSelector.PfxPath.CurrentValue;
                    }

                    if (this.CertificateSelector.Store.IsTouched || this.CertificateSelector.Password.IsTouched)
                    {
                        var encoder = new Crypto();
                        newConfiguration.Signing.EncodedPassword = encoder.Protect(this.CertificateSelector.Password.CurrentValue);
                    }
                }
                else
                {
                    newConfiguration.Signing.EncodedPassword = null;
                    newConfiguration.Signing.PfxPath = null;
                }

                if (this.CertificateSelector.Store.CurrentValue == CertificateSource.Personal)
                {
                    if (this.CertificateSelector.Store.IsTouched || this.CertificateSelector.SelectedPersonalCertificate.IsTouched)
                    {
                        newConfiguration.Signing.Thumbprint = this.CertificateSelector.SelectedPersonalCertificate.CurrentValue?.Model?.Thumbprint;
                    }
                }
                else
                {
                    newConfiguration.Signing.Thumbprint = null;
                }

                if (this.CertificateSelector.Store.CurrentValue == CertificateSource.DeviceGuard)
                {
                    if (this.CertificateSelector.Store.IsTouched || this.CertificateSelector.DeviceGuard.IsTouched)
                    {
                        newConfiguration.Signing.DeviceGuard = this.CertificateSelector.DeviceGuard.CurrentValue;
                    }
                }
                else
                {
                    newConfiguration.Signing.DeviceGuard = null;
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

            if (this.PowerShellEditorType.CurrentValue == EditorType.Custom)
            {
                if (this.ManifestEditorPath.IsTouched)
                {
                    newConfiguration.Editing.ManifestEditor.Resolved = this.ManifestEditorPath.CurrentValue;
                }
            }
            else
            {
                newConfiguration.Editing.ManifestEditor.Resolved = null;
            }

            if (this.AppinstallerEditorType.CurrentValue == EditorType.Custom)
            {
                if (this.AppinstallerEditorPath.IsTouched)
                {
                    newConfiguration.Editing.AppInstallerEditor.Resolved = this.AppinstallerEditorPath.CurrentValue;
                }
            }
            else
            {
                newConfiguration.Editing.AppInstallerEditor.Resolved = null;
            }

            if (this.MsixEditorType.CurrentValue == EditorType.Custom)
            {
                if (this.MsixEditorPath.IsTouched)
                {
                    newConfiguration.Editing.MsixEditor.Resolved = this.MsixEditorPath.CurrentValue;
                }
            }
            else
            {
                newConfiguration.Editing.MsixEditor.Resolved = null;
            }

            if (this.PsfEditorType.CurrentValue == EditorType.Custom)
            {
                if (this.PsfEditorPath.IsTouched)
                {
                    newConfiguration.Editing.PsfEditor.Resolved = this.PsfEditorPath.CurrentValue;
                }
            }
            else
            {
                newConfiguration.Editing.PsfEditor.Resolved = null;
            }

            if (this.PowerShellEditorType.CurrentValue == EditorType.Custom)
            {
                if (this.PowerShellEditorPath.IsTouched)
                {
                    newConfiguration.Editing.PowerShellEditor.Resolved = this.PowerShellEditorPath.CurrentValue;
                }
            }
            else
            {
                newConfiguration.Editing.PowerShellEditor.Resolved = null;
            }

            if (this.ManifestEditorType.CurrentValue == EditorType.Custom)
            {
                if (this.ManifestEditorPath.IsTouched)
                {
                    newConfiguration.Editing.ManifestEditor.Resolved = this.ManifestEditorPath.CurrentValue;
                }
            }
            else
            {
                newConfiguration.Editing.ManifestEditor.Resolved = null;
            }

            var toolsTouched = this.Tools.IsTouched;
            this.AllSettings.Commit();

            if (toolsTouched)
            {
                newConfiguration.Packages.Tools.Clear();
                newConfiguration.Packages.Tools.AddRange(this.Tools.Items.Select(t => (ToolListConfiguration)t));
            }

            await this.configurationService.SetCurrentConfigurationAsync(newConfiguration).ConfigureAwait(false);

            if (toolsTouched)
            {
                this.eventAggregator.GetEvent<ToolsChangedEvent>().Publish(this.Tools.Items.Select(t => (ToolListConfiguration)t).ToArray());
            }

            return true;
        }

        /// <inheritdoc />
#pragma warning disable CS0067
        public event Action<IDialogResult> RequestClose;
#pragma warning restore CS0067

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
