using System;
using System.Collections.Generic;
using System.Text;

namespace MSI_Hero.Domain.State
{
    public class LocalSettings : ILocalSettings
    {
        public bool ShowSidebar { get; set; }
    }
}
