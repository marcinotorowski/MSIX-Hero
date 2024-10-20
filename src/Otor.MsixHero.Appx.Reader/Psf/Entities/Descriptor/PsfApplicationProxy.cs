// MSIX Hero
// Copyright (C) 2024 Marcin Otorowski
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

namespace Otor.MsixHero.Appx.Reader.Psf.Entities.Descriptor;

[Serializable]
public class PsfApplicationProxy : BaseApplicationProxy
{
    public override ApplicationProxyType Type => ApplicationProxyType.PackageSupportFramework;

    public List<PsfFolderRedirectionDescriptor> FileRedirections { get; set; }

    public List<PsfScriptDescriptor> Scripts { get; set; }

    public PsfTracingRedirectionDescriptor Tracing { get; set; }

    public PsfElectronDescriptor Electron { get; set; }

    public List<string> OtherFixups { get; set; }
}