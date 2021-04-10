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
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Otor.MsixHero.Winget.Yaml.Entities;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.ObjectGraphVisitors;

namespace Otor.MsixHero.Winget.Yaml
{
    /// <summary>
    /// A class that writes YAML files for WinGet.
    /// </summary>
    public class YamlWriter
    {
        private readonly Lazy<ISerializer> serializer = new Lazy<ISerializer>(GetSerializer);
        
        /// <summary>
        /// Writes the given YAML definition to a stream and returns an asynchronous task.
        /// </summary>
        /// <param name="manifest">The YAML definition.</param>
        /// <param name="stream">The stream where to write the YAML content.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public async Task WriteAsync(YamlManifest manifest, Stream stream, CancellationToken cancellationToken = default)
        {
            await using var textWriter = new StreamWriter(stream, leaveOpen: true);
            await this.WriteAsync(manifest, textWriter, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Writes the given YAML definition to a given text writer and returns a task that represents the asynchronous operation.
        /// </summary>
        /// <param name="manifest">The YAML definition.</param>
        /// <param name="textWriter">The text writer where the content will be written.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public async Task WriteAsync(YamlManifest manifest, TextWriter textWriter, CancellationToken cancellationToken = default)
        {
            var stringBuilder = new StringBuilder();
            await using (var stringWriter = new StringWriter(stringBuilder))
            {
                this.serializer.Value.Serialize(stringWriter, manifest);
            }
            
            cancellationToken.ThrowIfCancellationRequested();

            stringBuilder.AppendLine();
            stringBuilder.AppendLine("# Edited with MSIX Hero");

            cancellationToken.ThrowIfCancellationRequested();
            var serialized = stringBuilder.ToString();
            serialized = Regex.Replace(serialized, @"[\r\n]{2,}", Environment.NewLine);
            await textWriter.WriteAsync(serialized).ConfigureAwait(false);
        }

        /// <summary>
        /// Writes the given YAML definition to a stream.
        /// </summary>
        /// <param name="manifest">The YAML definition.</param>
        /// <param name="stream">The stream to which the content will be written.</param>
        public void Write(YamlManifest manifest, Stream stream)
        {
            using var textWriter = new StreamWriter(stream, leaveOpen: true);
            this.Write(manifest, textWriter);
        }

        /// <summary>
        /// Writes the given YAML definition to a text writer.
        /// </summary>
        /// <param name="manifest">The YAML definition.</param>
        /// <param name="textWriter">The text writer where the content will be written.</param>
        public void Write(YamlManifest manifest, TextWriter textWriter)
        {
            var stringBuilder = new StringBuilder();
            using (var stringWriter = new StringWriter(stringBuilder))
            {
                this.serializer.Value.Serialize(stringWriter, manifest);
            }

            stringBuilder.AppendLine();
            stringBuilder.AppendLine("# Edited with MSIX Hero");

            var serialized = stringBuilder.ToString();
            serialized = Regex.Replace(serialized, @"[\r\n]{2,}", Environment.NewLine);

            textWriter.Write(serialized);
        }

        private static ISerializer GetSerializer()
        {
            var serializerBuilder = new SerializerBuilder()
                .WithEmissionPhaseObjectGraphVisitor(args => new DefaultExclusiveObjectGraphVisitor(args.InnerVisitor))
                .WithTypeConverter(new YamlStringEnumConverter());
            var serializer = serializerBuilder.Build();
            return serializer;
        }
    }
}