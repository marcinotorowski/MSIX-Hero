using System;
using System.Collections.Generic;
using System.Text;
using MSI_Hero.Domain.State.Enums;
using otor.msihero.lib;
using Prism.Events;

namespace MSI_Hero.Domain.Events
{
    public class PackagesLoadedEvent : PubSubEvent<PackageContext>
    {
    }
}
