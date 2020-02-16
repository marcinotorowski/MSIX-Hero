using System.Collections.Generic;
using otor.msixhero.lib.Infrastructure.Configuration;
using Prism.Events;

namespace otor.msixhero.lib.Domain.Events
{
    public class ToolsChangedEvent : PubSubEvent<IReadOnlyCollection<ToolListConfiguration>>
    {
    }
}
