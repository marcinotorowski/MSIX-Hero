using Otor.MsixHero.App.Hero;
using Otor.MsixHero.App.Hero.Commands.EventViewer;
using Otor.MsixHero.App.Hero.Events.Base;
using Otor.MsixHero.App.Mvvm;
using Prism.Events;

namespace Otor.MsixHero.App.Modules.EventViewer.ViewModels
{
    public class EventViewerSearchViewModel : NotifyPropertyChanged
    {
        private readonly IMsixHeroApplication application;

        public EventViewerSearchViewModel(IMsixHeroApplication application, IEventAggregator eventAggregator)
        {
            this.application = application;
            eventAggregator.GetEvent<UiExecutedEvent<SetEventViewerFilterCommand>>().Subscribe(this.OnSetEventViewerFilterCommand);
        }

        public string SearchKey
        {
            get => this.application.ApplicationState.EventViewer.SearchKey;

            set
            {
                if (this.application.ApplicationState.EventViewer.SearchKey == value)
                {
                    return;
                }

                var currentFilter = this.application.ApplicationState.EventViewer.Filter;
                this.application.CommandExecutor.Invoke(this, new SetEventViewerFilterCommand(currentFilter, value));
            }
        }

        private void OnSetEventViewerFilterCommand(UiExecutedPayload<SetEventViewerFilterCommand> obj)
        {
            this.OnPropertyChanged(nameof(this.SearchKey));
        }
    }
}
