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
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Otor.MsixHero.App.Modules.Dialogs.Settings.Context;
using Otor.MsixHero.App.Modules.Dialogs.Settings.Tabs;
using Otor.MsixHero.App.Mvvm;
using Otor.MsixHero.App.Mvvm.Changeable;
using Otor.MsixHero.Infrastructure.Localization;
using Otor.MsixHero.Infrastructure.Services;
using Prism.Services.Dialogs;

namespace Otor.MsixHero.App.Modules.Dialogs.Settings.ViewModel
{
    public class SettingsViewModel : NotifyPropertyChanged, IDialogAware
    {
        private readonly IConfigurationService _configurationService;
        private readonly IList<ISettingsComponent> _settingsTabs;
        private string _entryPoint;

        public SettingsViewModel(IConfigurationService configurationService)
        {
            this._configurationService = configurationService;
            this._settingsTabs = new List<ISettingsComponent>();
            
            MsixHeroTranslation.Instance.CultureChanged += (_, _) =>
            {
                this.OnPropertyChanged(nameof(this.Title));
            };

            this.Context = new SettingsContext();
            this.Context.ChangeableRegistered += (_, changeable) =>
            {
                changeable.IsValidated = false;
                this.AllSettings.AddChild(changeable);
                this._settingsTabs.Add(changeable);

                this.AllSettings.IsValidated = false;
            };
        }

        public SettingsContext Context { get; }

        public ChangeableContainer AllSettings { get; } = new();
        
        public string EntryPoint
        {
            get => this._entryPoint;
            private set => this.SetField(ref this._entryPoint, value);
        }

        public string Title => Resources.Localization.Dialogs_Settings_Title;

        public bool CanCloseDialog()
        {
            return this._settingsTabs.All(t => t.CanCloseDialog());
        }

        public void OnDialogClosed()
        {
            foreach (var settingsTab in this._settingsTabs)
            {
                settingsTab.OnDialogClosed();
            }
        }

        public void OnDialogOpened(IDialogParameters parameters)
        {
            if (!parameters.TryGetValue("tab", out string tab))
            {
                return;
            }

            this.EntryPoint = tab;

            foreach (var settingsTab in this._settingsTabs)
            {
                settingsTab.OnDialogOpened(parameters);
            }
        }

        public bool CanSave()
        {
            if (this.AllSettings.IsTouched && (!this.AllSettings.IsValidated || this.AllSettings.IsValid))
            {
                return true;
            }

            if (this._settingsTabs.Any(tab => !tab.CanSave()))
            {
                return false;
            }

            return false;
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

            foreach (var tab in this._settingsTabs)
            {
                if (!await tab.OnSaving(newConfiguration))
                {
                    return false;
                }
            }
            
            foreach (var tab in this._settingsTabs)
            {
                if (!await tab.OnSaving(newConfiguration).ConfigureAwait(false))
                {
                    return false;
                }
            }
            
            this.AllSettings.Commit();
            await this._configurationService.SetCurrentConfigurationAsync(newConfiguration).ConfigureAwait(false);
            return true;
        }

        /// <inheritdoc />
#pragma warning disable CS0067
        public event Action<IDialogResult> RequestClose;
#pragma warning restore CS0067
    }
}
