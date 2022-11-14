using System.Collections.ObjectModel;
using System.Globalization;
using System.Threading.Tasks;
using Otor.MsixHero.App.Helpers.Tiers;
using Otor.MsixHero.App.Modules.Dialogs.Settings.Context;
using Otor.MsixHero.App.Mvvm.Changeable;
using Otor.MsixHero.Elevation;
using Otor.MsixHero.Infrastructure.Configuration;
using Otor.MsixHero.Infrastructure.Helpers;
using Otor.MsixHero.Infrastructure.Localization;
using Otor.MsixHero.Infrastructure.Services;
using Prism.Services.Dialogs;

namespace Otor.MsixHero.App.Modules.Dialogs.Settings.Tabs.Interface.ViewModel
{
    public class InterfaceSettingsTabViewModel : ChangeableContainer, ISettingsComponent
    {
        private readonly IUacElevation _uacElevation;

        public InterfaceSettingsTabViewModel(
            IUacElevation uacElevation, 
            IConfigurationService configurationService,
            ITranslationProvider translationProvider)
        {
            var config = configurationService.GetCurrentConfiguration();

            this._uacElevation = uacElevation;

            this.AllLanguages.Add(LanguageViewModel.FromAuto());
            foreach (var translation in translationProvider.GetAvailableTranslations())
            {
                this.AllLanguages.Add(LanguageViewModel.FromCultureInfo(translation));
            }

            var uiLevel = (int)(config.UiConfiguration?.UxTier ?? UxTierLevel.Auto);
            if (uiLevel < -1 || uiLevel > 2)
            {
                uiLevel = -1;
            }

            var language = config.UiConfiguration?.Language;
            if (string.IsNullOrEmpty(language))
            {
                language = null;
            }

            this.AddChildren(
                this.ConfirmDeletion = new ChangeableProperty<bool>(config.UiConfiguration?.ConfirmDeletion != false),
                this.DefaultScreen = new ChangeableProperty<DefaultScreen>(config.UiConfiguration?.DefaultScreen ?? Infrastructure.Configuration.DefaultScreen.Packages),
                this.ShowReleaseNotes = new ChangeableProperty<bool>(config.Update?.HideNewVersionInfo != true),
                this.UxLevel = new ChangeableProperty<int>(uiLevel),
                this.Language = new ChangeableProperty<string>(language)
            );
        }

        public void Register(ISettingsContext context)
        {
            context.Register(this);
        }

        public ChangeableProperty<bool> ConfirmDeletion { get; }

        public ChangeableProperty<bool> ShowReleaseNotes { get; }

        public ChangeableProperty<int> UxLevel { get; }

        public ChangeableProperty<string> Language { get; }

        public ObservableCollection<LanguageViewModel> AllLanguages { get; } = new ObservableCollection<LanguageViewModel>();

        public ChangeableProperty<DefaultScreen> DefaultScreen { get; }
        
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
            if (this.DefaultScreen.IsTouched)
            {
                newConfiguration.UiConfiguration.DefaultScreen = this.DefaultScreen.CurrentValue;
            }

            if (this.ConfirmDeletion.IsTouched)
            {
                newConfiguration.UiConfiguration.ConfirmDeletion = this.ConfirmDeletion.CurrentValue;
            }

            if (this.ShowReleaseNotes.IsTouched)
            {
                newConfiguration.Update.HideNewVersionInfo = !this.ShowReleaseNotes.CurrentValue;
            }

            if (this.UxLevel.IsTouched)
            {
                newConfiguration.UiConfiguration.UxTier = (UxTierLevel)this.UxLevel.CurrentValue;
                if (newConfiguration.UiConfiguration.UxTier == UxTierLevel.Auto)
                {
                    TierController.SetSystemTier();
                }
                else
                {
                    TierController.SetCurrentTier(this.UxLevel.CurrentValue);
                }
            }

            if (this.Language.IsTouched)
            {
                newConfiguration.UiConfiguration.Language = this.Language.CurrentValue;

                CultureInfo newCulture;
                if (string.IsNullOrEmpty(this.Language.CurrentValue))
                {
                    newCulture = CultureInfo.InstalledUICulture;
                }
                else
                {
                    newCulture = ExceptionGuard.Guard(() => CultureInfo.GetCultureInfo(this.Language.CurrentValue));
                }

                // this is to ensure that in case of a running elevated proxy that is gets signaled we have now a new culture to consider.
                this._uacElevation.AsHighestAvailable<IMsixHeroTranslationService>().ChangeCulture(newCulture);
                MsixHeroTranslation.Instance.ChangeCulture(newCulture);
            }

            return Task.FromResult(true);
        }
    }
}
