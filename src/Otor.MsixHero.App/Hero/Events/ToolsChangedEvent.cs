using System.Collections.Generic;
using Otor.MsixHero.Infrastructure.Configuration;
using Prism.Events;

namespace Otor.MsixHero.App.Hero.Events
{
    public class ToolsChangedEvent : PubSubEvent<IReadOnlyCollection<ToolListConfiguration>>
    {
    }
}
