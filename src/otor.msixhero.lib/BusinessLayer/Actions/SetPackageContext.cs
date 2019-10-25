using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using otor.msixhero.lib.BusinessLayer.State.Enums;

namespace otor.msixhero.lib.BusinessLayer.Actions
{
    [Serializable]
    public class SetPackageContext : BaseAction
    {
        public SetPackageContext()
        {

        }

        public SetPackageContext(PackageContext context, bool force = false)
        {
            Context = context;
        }

        [XmlElement]
        public PackageContext Context { get; set; }

        [XmlElement]
        public bool Force { get; set; }
    }
}
