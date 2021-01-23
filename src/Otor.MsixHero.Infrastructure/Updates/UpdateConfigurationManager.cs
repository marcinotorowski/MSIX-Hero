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
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Otor.MsixHero.Infrastructure.Configuration;
using Otor.MsixHero.Infrastructure.Services;

namespace Otor.MsixHero.Infrastructure.Updates
{
    public class UpdateConfigurationManager : IUpdateConfigurationManager
    {
        private readonly IConfigurationService configurationService;
        private readonly IUpdateChecker updateChecker;

        public UpdateConfigurationManager(IConfigurationService configurationService, IUpdateChecker updateChecker)
        {
            this.configurationService = configurationService;
            this.updateChecker = updateChecker;
        }

        public Task<UpdateCheckResult> GetReleaseNotes(CancellationToken cancellationToken = default)
        {
            return this.updateChecker.CheckForNewVersion();
        }

        public async Task<bool> ShouldShowReleaseNotes(bool markReleaseNotesAsShown = true, CancellationToken cancellation = default)
        {
            var config = await this.configurationService.GetCurrentConfigurationAsync(token: cancellation).ConfigureAwait(false);
            if (config?.Update == null)
            {
                if (markReleaseNotesAsShown)
                {
                    await this.SetLastVersion(null, cancellation).ConfigureAwait(false);
                }

                return true;
            }

            // regardless of whether there was an update or not, do not show anything
            if (config.Update.HideNewVersionInfo)
            {
                if (markReleaseNotesAsShown)
                {
                    await this.SetLastVersion(null, cancellation).ConfigureAwait(false);
                }

                return false;
            }

            var lastVersion = config.Update.LastShownVersion;
            // last version is not provided
            if (string.IsNullOrEmpty(lastVersion) || !Version.TryParse(lastVersion, out var lastVersionParsed))
            {
                if (markReleaseNotesAsShown)
                {
                    await this.SetLastVersion(null, cancellation).ConfigureAwait(false);
                }

                return true;
            }

            // current version not specified
            // ReSharper disable once PossibleNullReferenceException
            var currentVersion = Version.Parse((Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly()).GetName().Version.ToString(3));
            
            // last version was greater or equal than the current one
            if (currentVersion <= lastVersionParsed)
            {
                if (markReleaseNotesAsShown)
                {
                    await this.SetLastVersion(null, cancellation).ConfigureAwait(false);
                }

                return false;
            }

            if (markReleaseNotesAsShown)
            {
                await this.SetLastVersion(null, cancellation).ConfigureAwait(false);
            }

            return !config.Update.HideNewVersionInfo;
        }

        public Task DisableReleaseNotes(CancellationToken token = default)
        {
            return this.SetLastVersion(true, token);
        }
        
        private async Task SetLastVersion(bool? dismissFutureNotifications = null, CancellationToken cancellation = default)
        {
            var currentVersion = (Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly()).GetName().Version;
            if (currentVersion == null)
            {
                return;
            }

            var changeRequired = false;
            var currentConfig = await this.configurationService.GetCurrentConfigurationAsync().ConfigureAwait(false);
            if (currentConfig == null)
            {
                changeRequired = true;
                currentConfig = new Configuration.Configuration();
            }

            if (currentConfig.Update == null)
            {
                changeRequired = true;
                currentConfig.Update = new UpdateConfiguration();
            }

            if (dismissFutureNotifications.HasValue)
            {
                changeRequired = currentConfig.Update.HideNewVersionInfo != dismissFutureNotifications.Value;
                currentConfig.Update.HideNewVersionInfo = dismissFutureNotifications.Value;
            }

            changeRequired |= !string.Equals(currentConfig.Update.LastShownVersion, currentVersion.ToString(), StringComparison.OrdinalIgnoreCase);
            currentConfig.Update.LastShownVersion = currentVersion.ToString();

            if (changeRequired)
            {
                await this.configurationService.SetCurrentConfigurationAsync(currentConfig, cancellation).ConfigureAwait(false);
            }
        }
    }
}
