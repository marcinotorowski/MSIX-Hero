using System.Threading.Tasks;
using Otor.MsixHero.App.Modules.Dialogs.Settings.Context;
using Otor.MsixHero.App.Mvvm.Changeable;
using Otor.MsixHero.Infrastructure.Configuration;
using Otor.MsixHero.Infrastructure.Services;
using Prism.Services.Dialogs;

namespace Otor.MsixHero.App.Modules.Dialogs.Settings.Tabs.Editors.ViewModel
{
    public class EditorSettingsTabViewModel : ChangeableContainer, ISettingsComponent
    {
        public EditorSettingsTabViewModel(IConfigurationService configurationService, IInteractionService interactionService)
        {
            var config = configurationService.GetCurrentConfiguration();

            this.AddChildren(
                this.ManifestEditorType = new ChangeableProperty<EditorType>(config.Editing.ManifestEditorType),
                this.ManifestEditorPath = new ChangeableFileProperty(() => Resources.Localization.Dialogs_Settings_Editors_Manifest_Path, interactionService, config.Editing.ManifestEditor.Resolved, this.ValidateManifestEditorPath),
                this.MsixEditorType = new ChangeableProperty<EditorType>(config.Editing.MsixEditorType),
                this.MsixEditorPath = new ChangeableFileProperty(() => Resources.Localization.Dialogs_Settings_Editors_Msix_Path, interactionService, config.Editing.MsixEditor.Resolved, this.ValidateMsixEditorPath),
                this.AppinstallerEditorType = new ChangeableProperty<EditorType>(config.Editing.AppInstallerEditorType),
                this.AppinstallerEditorPath = new ChangeableFileProperty(() => Resources.Localization.Dialogs_Settings_Editors_AppInstaller_Path, interactionService, config.Editing.AppInstallerEditor.Resolved, this.ValidateAppInstallerEditorPath),
                this.PsfEditorType = new ChangeableProperty<EditorType>(config.Editing.PsfEditorType),
                this.PsfEditorPath = new ChangeableFileProperty(() => Resources.Localization.Dialogs_Settings_Editors_Psf_Path, interactionService, config.Editing.PsfEditor.Resolved, this.ValidatePsfEditorPath),
                this.PowerShellEditorType = new ChangeableProperty<EditorType>(config.Editing.PowerShellEditorType),
                this.PowerShellEditorPath = new ChangeableFileProperty(() => Resources.Localization.Dialogs_Settings_Editors_Ps1_Path, interactionService, config.Editing.PowerShellEditor.Resolved, this.ValidatePowerShellEditorPath)
            );

            this.AppinstallerEditorType.ValueChanged += this.TypeOfPathChanged;
            this.ManifestEditorType.ValueChanged += this.TypeOfPathChanged;
            this.PsfEditorType.ValueChanged += this.TypeOfPathChanged;
            this.MsixEditorType.ValueChanged += this.TypeOfPathChanged;
            this.PowerShellEditorType.ValueChanged += this.TypeOfPathChanged;
        }

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

            return Task.FromResult(true);
        }

        public string ValidatePowerShellEditorPath(string value)
        {
            if (this.PowerShellEditorType.CurrentValue != EditorType.Custom)
            {
                return null;
            }

            return string.IsNullOrEmpty(value) ? Resources.Localization.Dialogs_Settings_Editors_Validation_EmptyPath : null;
        }

        public string ValidateManifestEditorPath(string value)
        {
            if (this.ManifestEditorType.CurrentValue != EditorType.Custom)
            {
                return null;
            }

            return string.IsNullOrEmpty(value) ? Resources.Localization.Dialogs_Settings_Editors_Validation_EmptyPath : null;
        }

        public string ValidateAppInstallerEditorPath(string value)
        {
            if (this.AppinstallerEditorType.CurrentValue != EditorType.Custom)
            {
                return null;
            }

            return string.IsNullOrEmpty(value) ? Resources.Localization.Dialogs_Settings_Editors_Validation_EmptyPath : null;
        }

        public string ValidateMsixEditorPath(string value)
        {
            if (this.MsixEditorType.CurrentValue != EditorType.Custom)
            {
                return null;
            }

            return string.IsNullOrEmpty(value) ? Resources.Localization.Dialogs_Settings_Editors_Validation_EmptyPath : null;
        }

        public string ValidatePsfEditorPath(string value)
        {
            if (this.PsfEditorType.CurrentValue != EditorType.Custom)
            {
                return null;
            }

            return string.IsNullOrEmpty(value) ? Resources.Localization.Dialogs_Settings_Editors_Validation_EmptyPath : null;
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
    }
}
