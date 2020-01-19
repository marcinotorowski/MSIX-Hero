using System.Collections.Generic;

namespace otor.msixhero.lib.Infrastructure.Update
{
    public class Changelog
    {
        public string Version { get; set; }

        public string Released { get; set; }

        public List<string> ReleaseNotes { get; set; }
    }
}