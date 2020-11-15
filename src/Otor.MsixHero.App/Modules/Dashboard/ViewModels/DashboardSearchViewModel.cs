using Otor.MsixHero.App.Hero;
using Otor.MsixHero.App.Hero.Commands.Dashboard;
using Otor.MsixHero.App.Hero.Events.Base;
using Otor.MsixHero.App.Mvvm;
using Prism.Events;

namespace Otor.MsixHero.App.Modules.Dashboard.ViewModels
{
    public class DashboardSearchViewModel : NotifyPropertyChanged
    {
        private readonly IMsixHeroApplication application;

        public DashboardSearchViewModel(IMsixHeroApplication application, IEventAggregator eventAggregator)
        {
            this.application = application;
            eventAggregator.GetEvent<UiExecutedEvent<SetToolFilterCommand>>().Subscribe(this.OnSetToolFilterCommand);
        }

        public string SearchKey
        {
            get => this.application.ApplicationState.Dashboard.SearchKey;

            set
            {
                if (this.application.ApplicationState.Dashboard.SearchKey == value)
                {
                    return;
                }

                this.application.CommandExecutor.Invoke(this, new SetToolFilterCommand(value));
            }
        }

        private void OnSetToolFilterCommand(UiExecutedPayload<SetToolFilterCommand> obj)
        {
            this.OnPropertyChanged(nameof(this.SearchKey));
        }
    }
}
