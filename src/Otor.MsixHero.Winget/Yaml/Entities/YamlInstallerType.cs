// MSIX Hero
// Copyright (C) 2021 Marcin Otorowski
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

namespace Otor.MsixHero.Winget.Yaml.Entities
{
    public enum YamlInstallerType
    {
        // ReSharper disable once InconsistentNaming
        none = 0,

        // ReSharper disable once InconsistentNaming
        exe,

        // ReSharper disable once InconsistentNaming
        msi,

        // ReSharper disable once InconsistentNaming
        msix,

        // ReSharper disable once InconsistentNaming
        // ReSharper disable once IdentifierTypo
        inno,
        
        // ReSharper disable once InconsistentNaming
        wix,
        
        // ReSharper disable once InconsistentNaming
        // ReSharper disable once IdentifierTypo
        nullsoft,
        
        // ReSharper disable once InconsistentNaming
        appx
    }
}