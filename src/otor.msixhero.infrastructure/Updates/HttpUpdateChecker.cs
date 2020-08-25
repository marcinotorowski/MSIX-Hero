using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Otor.MsixHero.Infrastructure.Updates
{
    public class HttpUpdateChecker : IUpdateChecker
    {
        public async Task<UpdateCheckResult> CheckForNewVersion(Version currentVersion)
        {
            var lastDefinition = await this.GetUpdateDefinition().ConfigureAwait(false);

            if (!Version.TryParse(lastDefinition.LastVersion ?? string.Empty, out var lastVersion))
            {
                throw new FormatException($"Version {lastDefinition.LastVersion} could not be parsed as a version string.");
            }

            return new UpdateCheckResult(currentVersion, lastVersion, lastDefinition.Released)
            {
                BlogUrl = lastDefinition.BlogUrl,
                Changes = lastDefinition.Changes
            };
        }

        public Task<UpdateCheckResult> CheckForNewVersion()
        {
            var assemblyVersion = FileVersionInfo.GetVersionInfo((Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly()).Location).ProductVersion;
            if (!Version.TryParse(assemblyVersion, out var version))
            {
                throw new FormatException($"Version {version} could not be parsed as a version string.");
            }

            return this.CheckForNewVersion(version);
        }

        private async Task<UpdateDefinition> GetUpdateDefinition()
        {
            var webRequest = WebRequest.CreateHttp("https://msixhero.net/update.json");
            using var webResponse = await webRequest.GetResponseAsync().ConfigureAwait(false);
            using (var stream = webResponse.GetResponseStream())
            {
                if (stream == null)
                {
                    throw new InvalidOperationException("Could not get information about the update.");
                }

                using var stringReader = new StreamReader(stream);
                var json = await stringReader.ReadToEndAsync().ConfigureAwait(false);
                var deserialized = JsonConvert.DeserializeObject<UpdateDefinition>(json);
                return deserialized;
            }
        }
    }
}
