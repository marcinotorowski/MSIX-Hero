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
using System.Threading;
using System.Threading.Tasks;
using Otor.MsixHero.Infrastructure.Configuration;
using Otor.MsixHero.Infrastructure.Services;

namespace Otor.MsixHero.App.Helpers.Update
{
    public class ReleaseNotesHelper
    {
        private readonly IConfigurationService configService;

        public ReleaseNotesHelper(IConfigurationService configService)
        {
            this.configService = configService;
        }

        public string GetCurrentVersion()
        {
            // ReSharper disable once PossibleNullReferenceException
            return typeof(ReleaseNotesHelper).Assembly.GetName().Version.ToString(3);
        }

        public async Task<bool> ShouldShowInitialReleaseNotesAsync(CancellationToken cancellationToken = default)
        {
            var cfg = await this.configService.GetCurrentConfigurationAsync(token: cancellationToken).ConfigureAwait(false);
            return this.ShouldShowInitialReleaseNotes(cfg.Update);
        }

        public bool ShouldShowInitialReleaseNotes()
        {
            var cfg = this.configService.GetCurrentConfiguration();
            return this.ShouldShowInitialReleaseNotes(cfg.Update);
        }

        public void SaveReleaseNotesConfig(bool showOnStart = true)
        {
            var config = this.configService.GetCurrentConfiguration(false);
            if (config.Update == null)
            {
                config.Update = new UpdateConfiguration();
            }

            config.Update.HideNewVersionInfo = !showOnStart;
            config.Update.LastShownVersion = this.GetCurrentVersion();

            this.configService.SetCurrentConfiguration(config);
        }

        public async Task SaveReleaseNotesConfigAsync(bool showOnStart = true, CancellationToken cancellationToken = default)
        {
            var config = await this.configService.GetCurrentConfigurationAsync(false, cancellationToken);
            if (config.Update == null)
            {
                config.Update = new UpdateConfiguration();
            }

            config.Update.HideNewVersionInfo = !showOnStart;
            config.Update.LastShownVersion = this.GetCurrentVersion();

            await this.configService.SetCurrentConfigurationAsync(config, cancellationToken).ConfigureAwait(false);
        }

        private bool ShouldShowInitialReleaseNotes(UpdateConfiguration updateConfig)
        {
            if (updateConfig == null)
            {
                return true;
            }

            if (updateConfig.HideNewVersionInfo)
            {
                return false;
            }

            var lastVersion = updateConfig.LastShownVersion;
            if (this.CompareCurrentVersionTo(lastVersion) <= 0) // current version is the same or lower than the last version
            {
                return false;
            }

            return true;
        }

        private int CompareCurrentVersionTo(Version version)
        {
            var currentVersion = typeof(ReleaseNotesHelper).Assembly.GetName().Version;
            if (currentVersion == null)
            {
                return -1;
            }

            currentVersion = new Version(currentVersion.Major, currentVersion.Minor, currentVersion.Build);

            return currentVersion.CompareTo(version);
        }

        private int CompareCurrentVersionTo(string version)
        {
            if (!Version.TryParse(version, out var parsed))
            {
                return -1;
            }

            return this.CompareCurrentVersionTo(parsed);
        }
    }
}
