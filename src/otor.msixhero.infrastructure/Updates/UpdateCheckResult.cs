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
