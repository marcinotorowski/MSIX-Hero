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

using System;
using System.Collections.Generic;

namespace Otor.MsixHero.Infrastructure.Updates
{
    public class UpdateCheckResult
    {
        internal UpdateCheckResult(Version currentVersion, Version availableVersion, DateTime releaseDate)
        {
            CurrentVersion = currentVersion;
            AvailableVersion = availableVersion;
            ReleaseDate = releaseDate;
        }

        public bool IsCurrentVersionUpToDate
        {
            get => this.CurrentVersion >= this.AvailableVersion;
        }

        public Version CurrentVersion { get; private set; }

        public Version AvailableVersion { get; private set; }

        public DateTime ReleaseDate { get; private set; }

        public string BlogUrl { get; set; }

        public List<ChangeLogItem> Changes { get; set; }
    }
}
