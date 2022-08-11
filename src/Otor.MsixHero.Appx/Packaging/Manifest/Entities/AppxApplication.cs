// MSIX Hero
// Copyright (C) 2022 Marcin Otorowski
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// Full notice:
// https://github.com/marcinotorowski/msix-hero/blob/develop/LICENSE.md

using System;
using System.Collections.Generic;
using Otor.MsixHero.Appx.Psf.Entities.Descriptor;

namespace Otor.MsixHero.Appx.Packaging.Manifest.Entities
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

        public string HostId { get; set; }

        public string Parameters { get; set; }

        public IList<string> ExecutionAlias { get; set; }

        public PsfApplicationDescriptor Psf { get; set; }

        public List<AppxExtension> Extensions { get; set; }
    }
}
