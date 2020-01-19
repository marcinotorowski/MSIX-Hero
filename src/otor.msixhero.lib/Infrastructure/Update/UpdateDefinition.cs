using System;

namespace otor.msixhero.lib.Infrastructure.Update
{
    public class UpdateDefinition
    {
        /*{
    "lastVersion": "0.9.0.0",
    "released": "2019-01-01"
}*/

        public string LastVersion { get; set; }

        public DateTime Released { get; set; }
    }
}
