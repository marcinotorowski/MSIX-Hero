// MSIX Hero
// Copyright (C) 2022 Marcin Otorowski
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// Full notice:
// https://github.com/marcinotorowski/msix-hero/blob/develop/LICENSE.md

using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using Otor.MsixHero.App.Controls.CertificateSelector.ViewModel;
using Otor.MsixHero.App.Helpers.Tiers;
using Otor.MsixHero.App.Hero.Events;
using Otor.MsixHero.App.Modules.Dialogs.Settings.ViewModel.Tools;
using Otor.MsixHero.App.Mvvm;
using Otor.MsixHero.App.Mvvm.Changeable;
using Otor.MsixHero.Appx.Signing.TimeStamping;
using Otor.MsixHero.Elevation;
using Otor.MsixHero.Infrastructure.Configuration;
using Otor.MsixHero.Infrastructure.Cryptography;
using Otor.MsixHero.Infrastructure.Logging;
using Otor.MsixHero.Infrastructure.Services;
using Prism.Events;
using Prism.Services.Dialogs;

namespace Otor.MsixHero.App.Modules.Dialogs.Settings.ViewModel
{
    public class SettingsViewModel : NotifyPropertyChanged, IDialogAware
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly IConfigurationService _configurationService;
        private string _entryPoint;

        public SettingsViewModel(
            IEventAggregator eventAggregator,
            IConfigurationService configurationService,
            IInteractionService interactionService,
            IUacElevation uacElevation,
            ITimeStampFeed timeStampFeed)
        {
            this._eventAggregator = eventAggregator;
            this._configurationService = configurationService;

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
                this.CertificateSelector = new CertificateSelectorViewModel(
                    interactionService, 
                    uacElevation, 
                    config.Signing,
                    timeStampFeed,
                    true)
            );

            var uiLevel = (int) (config.UiConfiguration?.UxTier ?? UxTierLevel.Auto);
            if (uiLevel < -1 || uiLevel > 2)
            {
                uiLevel = -1;
            }

            this.AllSettings.AddChildren(
                this.TabSigning,
                this.ConfirmDeletion = new ChangeableProperty<bool>(config.UiConfiguration?.ConfirmDeletion != false),
                this.DefaultScreen = new ChangeableProperty<DefaultScreen>(config.UiConfiguration == null ? Infrastructure.Configuration.DefaultScreen.Packages : config.UiConfiguration.DefaultScreen),
                this.ShowReleaseNotes = new ChangeableProperty<bool>(config.Update?.HideNewVersionInfo != true),
                this.UxLevel = new ChangeableProperty<int>(uiLevel),
                this.VerboseLogging = new ChangeableProperty<bool>(config.VerboseLogging),
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

            return string.IsNullOrEmpty(value) ? "The path cannot be empty." : null;
        }

        public string ValidateManifestEditorPath(string value)
        {
            if (this.ManifestEditorType.CurrentValue != EditorType.Custom)
            {
                return null;
            }

            return string.IsNullOrEmpty(value) ? "The path cannot be empty." : null;
        }

        public string ValidateAppInstallerEditorPath(string value)
        {
            if (this.AppinstallerEditorType.CurrentValue != EditorType.Custom)
            {
                return null;
            }

            return string.IsNullOrEmpty(value) ? "The path cannot be empty." : null;
        }

        public string ValidateMsixEditorPath(string value)
        {
            if (this.MsixEditorType.CurrentValue != EditorType.Custom)
            {
                return null;
            }

            return string.IsNullOrEmpty(value) ? "The path cannot be empty." : null;
        }

        public string ValidatePsfEditorPath(string value)
        {
            if (this.PsfEditorType.CurrentValue != EditorType.Custom)
            {
                return null;
            }

            return string.IsNullOrEmpty(value) ? "The path cannot be empty." : null;
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

        public ChangeableProperty<bool> VerboseLogging { get; }

        public ChangeableProperty<EditorType> MsixEditorType { get; }

        public ChangeableFileProperty MsixEditorPath { get; }

        public ChangeableProperty<EditorType> AppinstallerEditorType { get; }

        public ChangeableFileProperty AppinstallerEditorPath { get; }

        public ChangeableProperty<EditorType> PsfEditorType { get; }

        public ChangeableFileProperty PsfEditorPath { get; }

        public ChangeableProperty<EditorType> PowerShellEditorType { get; }

        public ChangeableFileProperty PowerShellEditorPath { get; }

        public ChangeableProperty<bool> PackerSignByDefault { get; }

        public ChangeableProperty<bool> ConfirmDeletion { get; }

        public ChangeableProperty<bool> ShowReleaseNotes { get; }
        
        public ChangeableProperty<int> UxLevel { get; }

        public ChangeableProperty<DefaultScreen> DefaultScreen { get; }

        public string EntryPoint
        {
            get => this._entryPoint;
            private set => this.SetField(ref this._entryPoint, value);
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
        private static MemberInfo GetMemberInfo(Expression expression)
        {
            var lambda = (LambdaExpression)expression;

            MemberExpression memberExpression;
            if (lambda.Body is UnaryExpression unaryExpression)
            {
                memberExpression = (MemberExpression)unaryExpression.Operand;
            }
            else
            {
                memberExpression = (MemberExpression)lambda.Body;
            }

            return memberExpression.Member;
        }
        
        private static void UpdateConfiguration<TObject, TValue>(TObject configObject, Expression<Func<TObject, TValue>> property, ChangeableProperty<TValue> value)
        {
            if (!value.IsTouched)
            {
                return;
            }
            
            var memberExpression = GetMemberInfo(property);
            var propertyName = memberExpression.Name;

            var propertyInfo = typeof(TObject).GetProperty(propertyName);
            if (propertyInfo == null)
            {
                throw new ArgumentException("Invalid property path.");
            }
            
            propertyInfo.SetValue(configObject, value.CurrentValue);
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

            var newConfiguration = await this._configurationService.GetCurrentConfigurationAsync(false).ConfigureAwait(false);
            
            UpdateConfiguration(newConfiguration.Signing, cfg => cfg.DefaultOutFolder, this.CertificateOutputPath);
            UpdateConfiguration(newConfiguration.Packer, cfg => cfg.SignByDefault, this.PackerSignByDefault);
            UpdateConfiguration(newConfiguration.UiConfiguration, e => e.DefaultScreen, this.DefaultScreen);
            UpdateConfiguration(newConfiguration.UiConfiguration, e => e.ConfirmDeletion, this.ConfirmDeletion);
            UpdateConfiguration(newConfiguration.Update, e => e.HideNewVersionInfo, this.ShowReleaseNotes);

            UpdateConfiguration(newConfiguration.Editing, e => e.ManifestEditorType, this.ManifestEditorType);
            UpdateConfiguration(newConfiguration.Editing, e => e.AppInstallerEditorType, this.AppinstallerEditorType);
            UpdateConfiguration(newConfiguration.Editing, e => e.MsixEditorType, this.MsixEditorType);
            UpdateConfiguration(newConfiguration.Editing, e => e.PsfEditorType, this.PsfEditorType);
            UpdateConfiguration(newConfiguration.Editing, e => e.PowerShellEditorType, this.PowerShellEditorType);
            
            UpdateConfiguration(newConfiguration.AppInstaller, e => e.DefaultRemoteLocationAppInstaller, this.DefaultRemoteLocationAppInstaller);
            UpdateConfiguration(newConfiguration.AppInstaller, e => e.DefaultRemoteLocationPackages, this.DefaultRemoteLocationPackages);

            if (this.VerboseLogging.IsTouched)
            {
                newConfiguration.VerboseLogging = this.VerboseLogging.CurrentValue;
                
                LogManager.Initialize(newConfiguration.VerboseLogging ? MsixHeroLogLevel.Trace : MsixHeroLogLevel.Info);
            }

            if (this.UxLevel.IsTouched)
            {
                newConfiguration.UiConfiguration.UxTier = (UxTierLevel) this.UxLevel.CurrentValue;
                if (newConfiguration.UiConfiguration.UxTier == UxTierLevel.Auto)
                {
                    TierController.SetSystemTier();
                }
                else
                {
                    TierController.SetCurrentTier(this.UxLevel.CurrentValue);
                }
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

                UpdateConfiguration(newConfiguration.Signing, e => e.Source, this.CertificateSelector.Store);


                string timeStampUrl;
                switch (this.CertificateSelector.TimeStampSelectionMode.CurrentValue)
                {
                    case TimeStampSelectionMode.None:
                        timeStampUrl = null;
                        break;
                    case TimeStampSelectionMode.Auto:
                        timeStampUrl = "auto";
                        break;
                    case TimeStampSelectionMode.Url:
                        timeStampUrl = this.CertificateSelector.TimeStamp.CurrentValue;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                newConfiguration.Signing.TimeStampServer = timeStampUrl;
            }

            if (this.ManifestEditorType.CurrentValue == EditorType.Custom)
            {
                UpdateConfiguration(newConfiguration.Editing.ManifestEditor, e => e.Resolved, this.ManifestEditorPath);
            }
            else
            {
                newConfiguration.Editing.ManifestEditor.Resolved = null;
            }

            if (this.AppinstallerEditorType.CurrentValue == EditorType.Custom)
            {
                UpdateConfiguration(newConfiguration.Editing.AppInstallerEditor, e => e.Resolved, this.AppinstallerEditorPath);
            }
            else
            {
                newConfiguration.Editing.AppInstallerEditor.Resolved = null;
            }

            if (this.MsixEditorType.CurrentValue == EditorType.Custom)
            {
                UpdateConfiguration(newConfiguration.Editing.MsixEditor, e => e.Resolved, this.MsixEditorPath);
            }
            else
            {
                newConfiguration.Editing.MsixEditor.Resolved = null;
            }

            if (this.PsfEditorType.CurrentValue == EditorType.Custom)
            {
                UpdateConfiguration(newConfiguration.Editing.PsfEditor, e => e.Resolved, this.PsfEditorPath);
            }
            else
            {
                newConfiguration.Editing.PsfEditor.Resolved = null;
            }

            if (this.PowerShellEditorType.CurrentValue == EditorType.Custom)
            {
                UpdateConfiguration(newConfiguration.Editing.PowerShellEditor, e => e.Resolved, this.PowerShellEditorPath);
            }
            else
            {
                newConfiguration.Editing.PowerShellEditor.Resolved = null;
            }
            
            var toolsTouched = this.Tools.IsTouched;
            this.AllSettings.Commit();

            if (toolsTouched)
            {
                newConfiguration.Packages.Tools.Clear();
                newConfiguration.Packages.Tools.AddRange(this.Tools.Items.Select(t => (ToolListConfiguration)t));
            }

            await this._configurationService.SetCurrentConfigurationAsync(newConfiguration).ConfigureAwait(false);

            if (toolsTouched)
            {
                this._eventAggregator.GetEvent<ToolsChangedEvent>().Publish(this.Tools.Items.Select(t => (ToolListConfiguration)t).ToArray());
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
