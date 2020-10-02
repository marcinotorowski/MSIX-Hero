using System.Collections.Generic;
using Otor.MsixHero.Infrastructure.Configuration;
using Prism.Events;

namespace Otor.MsixHero.Ui.Hero.Events
{
    public class ToolsChangedEvent : PubSubEvent<IReadOnlyCollection<ToolListConfiguration>>
    {
    }
}
