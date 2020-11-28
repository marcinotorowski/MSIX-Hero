using System;

namespace Otor.MsixHero.Appx.Diagnostic.Logging.Entities
{
    public class Log
    {
        public DateTime DateTime { get; set; }

        public string Message { get; set; }
        
        public Guid? ActivityId { get; set; }

        public string PackageName { get; set; }

        public string FilePath { get; set; }

        public string User { get; set; }

        public string Level { get; set; }

        public int ThreadId { get; set; }

        public string Source { get; set; }

        public string OpcodeDisplayName { get; set; }

        public string ErrorCode { get; set; }
    }
}