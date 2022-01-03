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
using System.Diagnostics;
using System.Reflection;
using System.Windows.Input;
using Otor.MsixHero.App.Helpers;
using Otor.MsixHero.App.Helpers.Update;
using Otor.MsixHero.App.Hero;
using Otor.MsixHero.App.Mvvm;
using Otor.MsixHero.Infrastructure.Configuration;
using Otor.MsixHero.Infrastructure.Helpers;
using Otor.MsixHero.Infrastructure.Services;
using Prism.Commands;
using Prism.Regions;

namespace Otor.MsixHero.App.Modules.WhatsNew.ViewModels
{
    public class WhatsNewViewModel : NotifyPropertyChanged, INavigationAware
    {
        private readonly IMsixHeroApplication application;
        private readonly IInteractionService interactionService;
        private readonly IConfigurationService configurationService;
        private bool showUpdateScreen;

        public WhatsNewViewModel(
            IMsixHeroApplication application, 
            IInteractionService interactionService,
            IConfigurationService configurationService)
        {
            this.application = application;
            this.interactionService = interactionService;
            this.configurationService = configurationService;
            this.Dismiss = new DelegateCommand(this.OnDismiss);
            this.OpenReleaseNotes = new DelegateCommand(this.OnOpenReleaseNotes);
        }

        private void OnOpenReleaseNotes()
        {
            ExceptionGuard.Guard(() =>
            {
                var psi = new ProcessStartInfo("https://msixhero.net/redirect/release-notes/" + this.CurrentVersion)
                {
                    UseShellExecute = true
                };

                Process.Start(psi);
            },
            this.interactionService);
        }

        private void OnDismiss()
        {
            var initialScreenHelper = new InitialScreen(this.application, this.configurationService);
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

        public string Caption { get; private set; }

        public ICommand OpenReleaseNotes
        {
            get;
        }

        public bool ShowUpdateScreen
        {
            get => this.showUpdateScreen;
            set => this.SetField(ref this.showUpdateScreen, value);
        }

        void INavigationAware.OnNavigatedTo(NavigationContext navigationContext)
        {
            var cfg = this.configurationService.GetCurrentConfiguration();
            this.showUpdateScreen = cfg.Update?.HideNewVersionInfo != true;

            var startScreen = cfg.UiConfiguration?.DefaultScreen ?? DefaultScreen.Packages;
            switch (startScreen)
            {
                case DefaultScreen.Dashboard:
                    this.Caption = "Dismiss and go to the Dashboard";
                    break;
                case DefaultScreen.Packages:
                    this.Caption = "Dismiss and go to the list of installed apps";
                    break;
                case DefaultScreen.Volumes:
                    this.Caption = "Dismiss and go to the list of volumes";
                    break;
                case DefaultScreen.Events:
                    this.Caption = "Dismiss and go to MSIX logs";
                    break;
                case DefaultScreen.System:
                    this.Caption = "Dismiss and go to the System status";
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            this.OnPropertyChanged(nameof(ShowUpdateScreen));
            this.OnPropertyChanged(nameof(Caption));
        }

        bool INavigationAware.IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        void INavigationAware.OnNavigatedFrom(NavigationContext navigationContext)
        {
            return;
            if (navigationContext.Uri.OriginalString == NavigationPaths.WhatsNew)
            {
                return;
            }

            this.interactionService.ShowToast("Info about the new version", "You can find these information later in the ABOUT menu (bottom left corner).");
            var releaseNotesHelper = new ReleaseNotesHelper(this.configurationService);
            releaseNotesHelper.SaveReleaseNotesConfig(this.showUpdateScreen);
        }
    }
}
