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
