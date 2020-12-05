using System.Windows.Input;
using Otor.MsixHero.App.Hero;
using Otor.MsixHero.App.Hero.Commands;
using Otor.MsixHero.App.Hero.State;
using Otor.MsixHero.App.Mvvm;
using Otor.MsixHero.Infrastructure.Services;
using Prism.Commands;
using Prism.Regions;

namespace Otor.MsixHero.App.Modules.WhatsNew.ViewModels
{
    public class WhatsNewViewModel : NotifyPropertyChanged, INavigationAware
    {
        private readonly IMsixHeroApplication application;
        private readonly IInteractionService interactionService;

        public WhatsNewViewModel(IMsixHeroApplication application, IInteractionService interactionService)
        {
            this.application = application;
            this.interactionService = interactionService;
            this.Dismiss = new DelegateCommand(this.OnDismiss);
        }

        private void OnDismiss()
        {
            this.application.CommandExecutor.Invoke(this, new SetCurrentModeCommand(ApplicationMode.Packages));
        }

        public ICommand Dismiss
        {
            get;
        }

        void INavigationAware.OnNavigatedTo(NavigationContext navigationContext)
        {
        }

        bool INavigationAware.IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        void INavigationAware.OnNavigatedFrom(NavigationContext navigationContext)
        {
            this.interactionService.ShowToast("Info about the new version", "You can find these information later in the ABOUT menu (bottom left corner).", InteractionType.Information);
        }
    }
}
