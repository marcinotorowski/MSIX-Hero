using Otor.MsixHero.App.Hero;
using Otor.MsixHero.App.Hero.Commands.Packages;
using Otor.MsixHero.App.Hero.Events.Base;
using Otor.MsixHero.App.Mvvm;

namespace Otor.MsixHero.App.Modules.Packages.ViewModels
{
    public class PackagesSearchViewModel : NotifyPropertyChanged
    {
        private readonly IMsixHeroApplication application;

        public PackagesSearchViewModel(IMsixHeroApplication application)
        {
            this.application = application;
            this.application.EventAggregator.GetEvent<UiExecutedEvent<SetPackageFilterCommand>>().Subscribe(this.OnSetPackageFilterCommand);
        }

        private void OnSetPackageFilterCommand(UiExecutedPayload<SetPackageFilterCommand> obj)
        {
            this.OnPropertyChanged(nameof(SearchKey));
        }


        public string SearchKey
        {
            get => this.application.ApplicationState.Packages.SearchKey;
            set => this.application.CommandExecutor.Invoke(this, new SetPackageFilterCommand(this.application.CommandExecutor.ApplicationState.Packages.PackageFilter, value));
        }
    }
}
