using System;

namespace Otor.MsixHero.Appx.Diagnostic.Logging.Entities
{
    public class Log
    {
        public DateTime DateTime { get; set; }

        public string Message { get; set; }
        
        public int ActivityId { get; set; }

        public string PackageName { get; set; }

        public string User { get; set; }

        public string Level { get; set; }

        public int ThreadId { get; set; }
    }
}