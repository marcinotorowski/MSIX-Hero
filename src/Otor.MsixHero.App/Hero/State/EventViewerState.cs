using Otor.MsixHero.Appx.Diagnostic.Logging.Entities;
using Otor.MsixHero.Infrastructure.Configuration;

namespace Otor.MsixHero.App.Hero.State
{
    public class EventViewerState
    {
        public string SearchKey { get; set; }

        public Log SelectedLog { get; set; }

        public EventSort SortMode { get; set; }
        
        public bool SortDescending { get; set; }
        
        public EventFilter Filter { get; set; }
    }
}