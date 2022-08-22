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
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Dapplo.Log;

namespace Otor.MsixHero.Appx.Signing.TimeStamping;

public class CachedTimeStampFeed : StreamTimeStampFeed
{
    private static readonly LogSource Logger = new();
    private readonly ITimeStampFeed decoratedFeed;
    private readonly TimeSpan invalidateAfter;
    private DateTime lastRead = DateTime.MinValue;
    private TimeStampFeedEntries cache;
    private readonly AutoResetEvent syncObject = new AutoResetEvent(true);

    public CachedTimeStampFeed(ITimeStampFeed decoratedFeed, TimeSpan invalidateAfter)
    {
        this.decoratedFeed = decoratedFeed;
        this.invalidateAfter = invalidateAfter;
    }

    public CachedTimeStampFeed(ITimeStampFeed decoratedFeed) : this(decoratedFeed, TimeSpan.FromHours(24))
    {
    }

    public override async Task<TimeStampFeedEntries> GetTimeStampServers(CancellationToken cancellationToken = default)
    {
        Logger.Debug().WriteLine("Getting exclusive lock for cached read operation…");
        var gotLock = this.syncObject.WaitOne(TimeSpan.FromSeconds(10));

        try
        {
            if (!gotLock)
            {
                Logger.Debug().WriteLine("Could not get an exclusive lock in 10 seconds, aborting…");
                return new TimeStampFeedEntries();
            }

            if (this.cache?.Servers != null)
            {
                if (this.lastRead > DateTime.Now.Subtract(this.invalidateAfter))
                {
                    Logger.Debug().WriteLine($"Returning the cached copy of servers (last modification {this.lastRead}) which is not older than {this.invalidateAfter} ago…");
                    return this.cache;
                }


                Logger.Debug().WriteLine($"Invalidating the cached copy of servers (last modification {this.lastRead}) which was older than {this.invalidateAfter} ago…");
                this.cache = null;
                this.lastRead = DateTime.MinValue;
            }

            var file = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData, Environment.SpecialFolderOption.DoNotVerify), "msix-hero", "timestamps.json");

            bool commit;
            if (File.Exists(file))
            {
                var lastDate = File.GetLastWriteTimeUtc(file);
                if (lastDate > DateTime.UtcNow.Subtract(this.invalidateAfter))
                {
                    Logger.Debug().WriteLine($"Returning the local copy of servers from file {file} (last modification {lastDate}) which is not older than {this.invalidateAfter} ago…");
                    // the file is in accepted date range, so we can deserialize it and return the results
                    await using var fs = await this.OpenStream(cancellationToken).ConfigureAwait(false);
                    this.cache = await this.GetTimeStampServers(fs, cancellationToken).ConfigureAwait(false);
                    commit = false;
                }
                else
                {
                    Logger.Debug().WriteLine($"Invalidating the cached copy of servers from file {file} (last modification {lastDate}) which is older than {this.invalidateAfter} ago…");
                    this.cache = await this.decoratedFeed.GetTimeStampServers(cancellationToken).ConfigureAwait(false);
                    this.lastRead = DateTime.UtcNow;
                    Logger.Info().WriteLine($"New last read date is {this.lastRead}.");
                    commit = true;
                }
            }
            else
            {
                Logger.Debug().WriteLine("There is no cached copy available, getting the list from the actual provider…");
                this.cache = await this.decoratedFeed.GetTimeStampServers(cancellationToken).ConfigureAwait(false);
                this.lastRead = DateTime.UtcNow;
                Logger.Info().WriteLine($"New last read date is {this.lastRead}.");
                commit = true;
            }

            if (commit)
            {
                var fileInfo = new FileInfo(file);
                if (fileInfo.Exists)
                {
                    Logger.Info().WriteLine($"Deleting existing cached copy {fileInfo.FullName}…");
                    fileInfo.Delete();
                }
                else if (fileInfo.Directory?.Exists == false)
                {
                    Logger.Info().WriteLine($"Creating directory {fileInfo.Directory.FullName}…");
                    fileInfo.Directory.Create();
                }

                Logger.Debug().WriteLine($"Serializing cached entries into JSON format…");
                var jsonString = JsonConvert.SerializeObject(this.cache, Formatting.Indented);
                Logger.Debug().WriteLine($"Writing cached entries in JSON format into {fileInfo.FullName}…");
                await File.WriteAllTextAsync(fileInfo.FullName, jsonString, cancellationToken).ConfigureAwait(false);
            }

            return this.cache;
        }
        finally
        {
            if (gotLock)
            {
                Logger.Debug().WriteLine("Returning from exclusive lock…");
                this.syncObject.Set();
            }
        }
    }

    protected override Task<Stream> OpenStream(CancellationToken cancellationToken)
    {
        var file = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData, Environment.SpecialFolderOption.DoNotVerify), "msix-hero", "timestamps.json");

        Logger.Debug().WriteLine($"Opening file {file}…");
        return Task.FromResult((Stream)File.OpenRead(file));
    }
}