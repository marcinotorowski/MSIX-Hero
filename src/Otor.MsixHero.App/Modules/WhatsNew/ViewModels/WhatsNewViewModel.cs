using System;
using System.Diagnostics;
using System.Reflection;
using System.Windows.Input;
using Otor.MsixHero.App.Helpers;
using Otor.MsixHero.App.Helpers.Update;
using Otor.MsixHero.App.Hero;
using Otor.MsixHero.App.Hero.Commands;
using Otor.MsixHero.App.Hero.State;
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
                    this.Caption = "Close this dialog and jump to the Dashboard";
                    break;
                case DefaultScreen.Packages:
                    this.Caption = "Close this dialog and jump to the Packages screen";
                    break;
                case DefaultScreen.Volumes:
                    this.Caption = "Close this dialog and jump to the Volumes screen";
                    break;
                case DefaultScreen.Events:
                    this.Caption = "Close this dialog and jump to the Event Viewer";
                    break;
                case DefaultScreen.System:
                    this.Caption = "Close this dialog and jump to the Packages screen";
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
