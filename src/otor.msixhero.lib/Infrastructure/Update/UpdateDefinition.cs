using System;
using System.Collections.Generic;

namespace otor.msixhero.lib.Infrastructure.Update
{
    public class UpdateDefinition
    {
        public string LastVersion { get; set; }

        public DateTime Released { get; set; }

        public string BlogUrl { get; set; }

        public List<ChangeLogItem> Changes { get; set; }
    }
}
