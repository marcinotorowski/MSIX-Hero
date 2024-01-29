// MSIX Hero
// Copyright (C) 2024 Marcin Otorowski
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

using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Dapplo.Log;

namespace Otor.MsixHero.Appx.Signing.TimeStamping;

public abstract class StreamTimeStampFeed : ITimeStampFeed
{
    private static readonly LogSource Logger = new();
    public virtual async Task<TimeStampFeedEntries> GetTimeStampServers(CancellationToken cancellationToken = default)
    {
        await using var stream = await this.OpenStream(cancellationToken);
        return await this.GetTimeStampServers(stream, cancellationToken).ConfigureAwait(false);
    }

    protected async Task<TimeStampFeedEntries> GetTimeStampServers(Stream stream, CancellationToken cancellationToken = default)
    {
        Logger.Debug().WriteLine("Parsing stream to JSON…");
        using var textReader = new StreamReader(stream);
        using var jsonReader = new JsonTextReader(textReader);
        var parsedJson = await JToken.LoadAsync(jsonReader, cancellationToken).ConfigureAwait(false);
        var definition = parsedJson.ToObject<TimeStampFeedEntries>();
        return definition;
    }

    protected abstract Task<Stream> OpenStream(CancellationToken cancellationToken);
}