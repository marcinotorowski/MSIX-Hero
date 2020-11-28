using System.Linq;
using Otor.MsixHero.App.Mvvm;
using Otor.MsixHero.Appx.Diagnostic.Logging.Entities;
using Prism.Regions;

namespace Otor.MsixHero.App.Modules.EventViewer.Details.ViewModels
{
    public class EventViewerDetailsViewModel : NotifyPropertyChanged, INavigationAware
    {
        private LogViewModel selectedLog;

        public EventViewerDetailsViewModel()
        {
        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            var newSelection = GetLogFromContext(navigationContext);

            if (newSelection == null)
            {
                this.SelectedLog = null;
            }
            else if (this.SelectedLog?.Model != newSelection)
            {
                this.SelectedLog = new LogViewModel(newSelection);
            }
        }

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
        }

        public LogViewModel SelectedLog
        {
            get => this.selectedLog;
            private set => this.SetField(ref this.selectedLog, value);
        }

        private static Log GetLogFromContext(NavigationContext context)
        {
            var key = context.Parameters.Keys.FirstOrDefault(k => context.Parameters[k] is Log);
            if (key == null)
            {
                return null;
            }

            return (Log) context.Parameters[key];
        }
    }
}

