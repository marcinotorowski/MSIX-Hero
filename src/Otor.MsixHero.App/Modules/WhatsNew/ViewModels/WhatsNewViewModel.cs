// MSIX Hero
// Copyright (C) 2024 Marcin Otorowski
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
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Windows.Input;
using Otor.MsixHero.App.Helpers;
using Otor.MsixHero.App.Helpers.Update;
using Otor.MsixHero.App.Hero;
using Otor.MsixHero.App.Mvvm;
using Otor.MsixHero.Infrastructure.Configuration;
using Otor.MsixHero.Infrastructure.Helpers;
using Otor.MsixHero.Infrastructure.Localization;
using Otor.MsixHero.Infrastructure.Services;
using Prism.Commands;
using Prism.Regions;

namespace Otor.MsixHero.App.Modules.WhatsNew.ViewModels
{
    public class WhatsNewViewModel : NotifyPropertyChanged, INavigationAware
    {
        private readonly IMsixHeroApplication _application;
        private readonly IInteractionService _interactionService;
        private readonly IConfigurationService _configurationService;
        private bool _showUpdateScreen;
        private DefaultScreen _currentScreen;

        public WhatsNewViewModel(
            IMsixHeroApplication application,
            IInteractionService interactionService,
            IConfigurationService configurationService)
        {
            this._application = application;
            this._interactionService = interactionService;
            this._configurationService = configurationService;
            this.Dismiss = new DelegateCommand(this.OnDismiss);
            this.OpenReleaseNotes = new DelegateCommand(this.OnOpenReleaseNotes);

            MsixHeroTranslation.Instance.CultureChanged += this.InstanceOnCultureChanged;
        }

        private void OnOpenReleaseNotes()
        {
            ExceptionGuard.Guard(() =>
            {
                var psi = new ProcessStartInfo("https://msixhero.net/category/release/")
                {
                    UseShellExecute = true
                };

                Process.Start(psi);
            },
            this._interactionService);
        }

        private void InstanceOnCultureChanged(object sender, CultureInfo e)
        {
            this.OnPropertyChanged(nameof(this.Caption));
        }

        private void OnDismiss()
        {
            var initialScreenHelper = new InitialScreen(this._application, this._configurationService);
            initialScreenHelper.GoToDefaultScreenAsync();
        }

        // ReSharper disable once PossibleNullReferenceException
        public string CurrentVersionLine => (Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly()).GetName().Version.ToString(2);

        // ReSharper disable once PossibleNullReferenceException
        public string CurrentVersion => (Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly()).GetName().Version.ToString(3);

        public ICommand Dismiss
        {
            get;
        }

        public string Caption
        {
            get
            {
                return this._currentScreen switch
                {
                    DefaultScreen.Tools => Resources.Localization.Dialogs_WhatsNew_SkipToTools,
                    DefaultScreen.Packages => Resources.Localization.Dialogs_WhatsNew_SkipToPackages,
                    DefaultScreen.Volumes => Resources.Localization.Dialogs_WhatsNew_SkipToVolumes,
                    DefaultScreen.Events => Resources.Localization.Dialogs_WhatsNew_SkipToEventViewer,
                    DefaultScreen.Containers => Resources.Localization.Dialogs_WhatsNew_SkipToContainers,
                    DefaultScreen.System => Resources.Localization.Dialogs_WhatsNew_SkipToStatus,
                    _ => throw new ArgumentOutOfRangeException()
                };
            }
        }

        public ICommand OpenReleaseNotes
        {
            get;
        }

        public bool ShowUpdateScreen
        {
            get => this._showUpdateScreen;
            set => this.SetField(ref this._showUpdateScreen, value);
        }

        void INavigationAware.OnNavigatedTo(NavigationContext navigationContext)
        {
            var cfg = this._configurationService.GetCurrentConfiguration();
            this._showUpdateScreen = cfg.Update?.HideNewVersionInfo != true;

            this._currentScreen = cfg.UiConfiguration?.DefaultScreen ?? DefaultScreen.Packages;

            this.OnPropertyChanged(nameof(ShowUpdateScreen));
            this.OnPropertyChanged(nameof(Caption));
        }

        bool INavigationAware.IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        void INavigationAware.OnNavigatedFrom(NavigationContext navigationContext)
        {
            if (navigationContext.Uri.OriginalString == NavigationPaths.WhatsNew)
            {
                return;
            }

            var releaseNotesHelper = new ReleaseNotesHelper(this._configurationService);
            releaseNotesHelper.SaveReleaseNotesConfig(this._showUpdateScreen);
        }
    }
}
