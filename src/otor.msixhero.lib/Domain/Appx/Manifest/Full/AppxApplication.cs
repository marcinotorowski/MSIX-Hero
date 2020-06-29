using System;
using System.Collections.Generic;
using System.Windows.Documents;
using otor.msixhero.lib.Domain.Appx.Psf;
using otor.msixhero.lib.Domain.Appx.Psf.Descriptor;

namespace otor.msixhero.lib.Domain.Appx.Manifest.Full
{
    [Serializable]
    public class AppxApplication
    {
        public string Description { get; set; }
        
        public string DisplayName { get; set; }

        public string EntryPoint { get; set; }

        public string Id { get; set; }

        public byte[] Logo { get; set; }

        public string Executable { get; set; }
        
        public string StartPage { get; set; }

        public string Square150x150Logo { get; set; }

        public string Square44x44Logo { get; set; }

        public string Square30x30Logo { get; set; }

        public string BackgroundColor { get; set; }
        
        public string Wide310x150Logo { get; set; }

        public string ShortName { get; set; }

        public string Square310x310Logo { get; set; }

        public string Square71x71Logo { get; set; }

        public bool Visible { get; set; }

        public IList<string> ExecutionAlias { get; set; }

        public PsfApplicationDescriptor Psf { get; set; }

        public List<AppxExtension> Extensions { get; set; }
    }
}
