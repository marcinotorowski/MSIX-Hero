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

using System.Collections.Generic;
using Otor.MsixHero.Appx.Updates.Entities.Appx;

namespace Otor.MsixHero.Appx.Updates.Entities.Comparison
{
    public class ChangedFiles : IImpactedFiles
    {
        public IList<AppxFile> OldPackageFiles { get; set; }
        
        public IList<AppxFile> NewPackageFiles { get; set; }
        
        public long FileCount { get; set; }
        
        public long OldPackageFileSize { get; set; }

        public long OldPackageBlockCount { get; set; }

        public long OldPackageBlockSize { get; set; }

        public long NewPackageFileSize { get; set; }
        
        public long NewPackageBlockCount { get; set; }
        
        public long NewPackageBlockSize { get; set; }

        public long UpdateImpact { get; set; }
        
        public long ActualUpdateImpact { get; set; }
        
        public long SizeDifference { get; set; }
    }
}