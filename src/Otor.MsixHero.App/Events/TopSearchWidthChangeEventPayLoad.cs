using Prism.Events;

namespace Otor.MsixHero.App.Events
{
    public class TopSearchWidthChangeEventPayLoad
    {
        public TopSearchWidthChangeEventPayLoad(double panelWidth)
        {
            this.PanelWidth = panelWidth;
        }

        public double PanelWidth { get; }
    }
    
    public class TopSearchWidthChangeEvent : PubSubEvent<TopSearchWidthChangeEventPayLoad>
    {
    }
}
