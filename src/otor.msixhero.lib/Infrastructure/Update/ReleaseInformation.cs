using System;
using System.Collections.Generic;
using System.Text;
using Org.BouncyCastle.Asn1.Crmf;

namespace otor.msixhero.lib.Infrastructure.Update
{
    public class ReleaseInformation
    {
        public List<Changelog> Changelog { get; set; }
    }
}
