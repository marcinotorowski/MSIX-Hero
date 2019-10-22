using System;
using System.Collections.Generic;
using System.Text;
using Prism.Events;

namespace MSI_Hero.Domain.Events
{
    public class PackagesSidebarVisibilityChanged : PubSubEvent<bool>
    {
    }
}
