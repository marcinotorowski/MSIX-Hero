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
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Otor.MsixHero.Winget.Yaml.Entities;
using YamlDotNet.Serialization;

namespace Otor.MsixHero.Winget.Yaml
{
    /// <summary>
    /// A class used to read WinGet definition from a YAML file.
    /// </summary>
    public class YamlReader
    {
        private readonly Lazy<IDeserializer> deserializer = new Lazy<IDeserializer>(GetDeserializer);
        
        /// <summary>
        /// Reads WinGet definition from a stream and returns a task representing the asynchronous operation.
        /// </summary>
        /// <param name="stream">The input stream.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The asynchronous task that represents the operation.</returns>
        public async Task<YamlManifest> ReadAsync(Stream stream, CancellationToken cancellationToken = default)
        {
            using var textReader = new StreamReader(stream, leaveOpen: true);
            return await this.ReadAsync(textReader, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Reads WinGet definition from a string and returns a text reader representing the asynchronous operation.
        /// </summary>
        /// <param name="textReader">The text reader.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The asynchronous task that represents the operation.</returns>
        public Task<YamlManifest> ReadAsync(TextReader textReader, CancellationToken cancellationToken = default)
        {
            return Task.Run(() => deserializer.Value.Deserialize<YamlManifest>(textReader), cancellationToken);
        }

        /// <summary>
        /// Reads WinGet definition from a stream.
        /// </summary>
        /// <param name="stream">The input stream.</param>
        /// <returns>The WinGet definition.</returns>
        public YamlManifest Read(Stream stream)
        {
            using var textReader = new StreamReader(stream, leaveOpen: true);
            return this.Read(textReader);
        }


        /// <summary>
        /// Reads WinGet definition from a text reader.
        /// </summary>
        /// <param name="textReader">The input textReader.</param>
        /// <returns>The WinGet definition.</returns>
        public YamlManifest Read(TextReader textReader)
        {
            return deserializer.Value.Deserialize<YamlManifest>(textReader);
        }

        private static IDeserializer GetDeserializer()
        {
            var serializerBuilder = new DeserializerBuilder()
                .IgnoreUnmatchedProperties()
                .WithTypeConverter(new YamlStringEnumConverter());
            var serializer = serializerBuilder.Build();
            return serializer;
        }
    }
}
