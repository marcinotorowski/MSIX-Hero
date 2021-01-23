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
        /// <summary>
        /// Writes the given YAML definition to a stream and returns an asynchronous task.
        /// </summary>
        /// <param name="definition">The YAML definition.</param>
        /// <param name="stream">The stream where to write the YAML content.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public async Task WriteAsync(YamlDefinition definition, Stream stream, CancellationToken cancellationToken = default)
        {
            await using var textWriter = new StreamWriter(stream, leaveOpen: true);
            await this.WriteAsync(definition, textWriter, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Writes the given YAML definition to a given text writer and returns a task that represents the asynchronous operation.
        /// </summary>
        /// <param name="definition">The YAML definition.</param>
        /// <param name="textWriter">The text writer where the content will be written.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public async Task WriteAsync(YamlDefinition definition, TextWriter textWriter, CancellationToken cancellationToken = default)
        {
            var serializerBuilder = new SerializerBuilder().WithEmissionPhaseObjectGraphVisitor(args => new DefaultExclusiveObjectGraphVisitor(args.InnerVisitor));
            var serializer = serializerBuilder.Build();
            
            var stringBuilder = new StringBuilder();
            using (var stringWriter = new StringWriter(stringBuilder))
            {
                serializer.Serialize(stringWriter, definition);
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
        /// <param name="definition">The YAML definition.</param>
        /// <param name="stream">The stream to which the content will be written.</param>
        public void Write(YamlDefinition definition, Stream stream)
        {
            using (var textWriter = new StreamWriter(stream, leaveOpen: true))
            {
                this.Write(definition, textWriter);
            }
        }

        /// <summary>
        /// Writes the given YAML definition to a text writer.
        /// </summary>
        /// <param name="definition">The YAML definition.</param>
        /// <param name="textWriter">The text writer where the content will be written.</param>
        public void Write(YamlDefinition definition, TextWriter textWriter)
        {
            var serializer = new Serializer();
            var stringBuilder = new StringBuilder();
            using (var stringWriter = new StringWriter(stringBuilder))
            {
                serializer.Serialize(stringWriter, definition);
            }

            stringBuilder.AppendLine();
            stringBuilder.AppendLine("# Edited with MSIX Hero");

            var serialized = stringBuilder.ToString();
            serialized = Regex.Replace(serialized, @"[\r\n]{2,}", Environment.NewLine);

            textWriter.Write(serialized);
        }
    }
}