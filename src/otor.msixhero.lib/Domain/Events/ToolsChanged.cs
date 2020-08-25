using System.Collections.Generic;
using Otor.MsixHero.Infrastructure.Configuration;
using Prism.Events;

namespace Otor.MsixHero.Lib.Domain.Events
{
    public class ToolsChangedEvent : PubSubEvent<IReadOnlyCollection<ToolListConfiguration>>
    {
    }
}
