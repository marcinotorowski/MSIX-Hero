using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Dapplo.Log;

namespace Otor.MsixHero.Appx.Signing.TimeStamping
{
    public class MsixHeroGistTimeStampFeed : StreamTimeStampFeed
    {
        private static readonly LogSource Logger = new();
        public const string Url = "https://gist.githubusercontent.com/marcinotorowski/0bed416d906aef48f17e56c8946648bb/raw/TimeStampServer.json";

        public static ITimeStampFeed CreateCached()
        {
            return new CachedTimeStampFeed(new MsixHeroGistTimeStampFeed());
        }

        public override async Task<TimeStampFeedEntries> GetTimeStampServers(CancellationToken cancellationToken = default)
        {
            // The following code makes sure that the list of servers is shown in a random order (not to prefer any specific server).
            var originalList = await base.GetTimeStampServers(cancellationToken).ConfigureAwait(false);
            if (originalList?.Servers?.Any() != true)
            {
                return originalList;
            }

            Logger.Debug().WriteLine("Randomizing the list of timestamp servers...");

            var randomList = new TimeStampFeedEntries
            {
                Servers = new List<TimeStampServerEntry>()
            };

            var rnd = new Random();
            while (originalList.Servers.Any())
            {
                var randomElement = rnd.Next(0, originalList.Servers.Count);
                randomList.Servers.Add(originalList.Servers[randomElement]);
                originalList.Servers.RemoveAt(randomElement);
            }

            return randomList;
        }

        protected override async Task<Stream> OpenStream(CancellationToken cancellationToken)
        {
            Logger.Info().WriteLine($"Querying the list of available time stamp servers from {Url}...");
            using var httpClient = new HttpClient();
            using var response = await httpClient.GetAsync(Url, HttpCompletionOption.ResponseContentRead, cancellationToken).ConfigureAwait(false);

            var content = response.EnsureSuccessStatusCode();

            var memoryStream = new MemoryStream();
            var stream = await content.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
            await stream.CopyToAsync(memoryStream, cancellationToken).ConfigureAwait(false);
            memoryStream.Seek(0, SeekOrigin.Begin);
            return memoryStream;
        }
    }
}
