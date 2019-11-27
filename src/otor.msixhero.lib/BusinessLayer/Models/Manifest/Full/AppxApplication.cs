using System;
using otor.msixhero.lib.BusinessLayer.Models.Psf;

namespace otor.msixhero.lib.BusinessLayer.Models.Manifest.Full
{
    [Serializable]
    public class AppxApplication
    {
        public string Description { get; set; }

        public string Name { get; set; }

        public string DisplayName { get; set; }

        public string EntryPoint { get; set; }

        public string Executable { get; set; }

        public string Id { get; set; }

        public string Logo { get; set; }

        public string SmallLogo { get; set; }

        public string StartPage { get; set; }

        public string Square150x150Logo { get; set; }

        public string Square30x30Logo { get; set; }

        public string BackgroundColor { get; set; }

        public string ForegroundText { get; set; }

        public string WideLogo { get; set; }

        public string Wide310x310Logo { get; set; }

        public string ShortName { get; set; }

        public string Square310x310Logo { get; set; }

        public string Square70x70Logo { get; set; }

        public string MinWidth { get; set; }

        public string Publisher { get; set; }

        public PsfApplicationDefinition Psf { get; set; }
    }
}
