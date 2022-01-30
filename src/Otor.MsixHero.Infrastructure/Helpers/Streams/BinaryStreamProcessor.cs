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
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Otor.MsixHero.Infrastructure.Helpers.Streams
{
    public class BinaryStreamProcessor
    {
        private readonly Stream stream;

        public BinaryStreamProcessor(Stream stream)
        {
            this.stream = stream;
        }

        /// <summary>
        /// Reads the object using asynchronous methods.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// A hot running task that returns the read object.
        /// </returns>
        public async Task<string> ReadString(CancellationToken cancellationToken = default)
        {
            using var binaryReader = new AsyncBinaryReader(this.stream, true);
            var msgLength = await binaryReader.ReadIntAsync(cancellationToken).ConfigureAwait(false);
            if (msgLength == 0)
            {
                return null;
            }

            var bytes = await binaryReader.ReadBytesAsync(msgLength, cancellationToken).ConfigureAwait(false);
            return Encoding.UTF8.GetString(bytes);
        }

        /// <summary>
        /// Reads the object using asynchronous methods.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// A hot running task that returns the read object.
        /// </returns>
        public async Task<byte[]> ReadBytes(CancellationToken cancellationToken = default)
        {
            using var binaryReader = new AsyncBinaryReader(this.stream, true);
            var msgLength = await binaryReader.ReadIntAsync(cancellationToken).ConfigureAwait(false);
            if (msgLength == 0)
            {
                return null;
            }

            var bytes = await binaryReader.ReadBytesAsync(msgLength, cancellationToken).ConfigureAwait(false);
            return bytes;
        }

        /// <summary>
        /// Reads the object using asynchronous methods.
        /// </summary>
        /// <typeparam name="T">The type of parameter.</typeparam>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// A hot running task that returns the read object.
        /// </returns>
        public async Task<T> Read<T>(CancellationToken cancellationToken = default)
        {
            using var binaryReader = new AsyncBinaryReader(this.stream, true);
            var msgLength = await binaryReader.ReadIntAsync(cancellationToken).ConfigureAwait(false);
            if (msgLength == 0)
            {
                return default;
            }

            var jsonString = await binaryReader.ReadStringAsync(msgLength, Encoding.UTF8, cancellationToken).ConfigureAwait(false);
            return JsonConvert.DeserializeObject<T>(jsonString, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto });
        }

        /// <summary>
        /// Reads the object using asynchronous methods.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// A hot running task that returns the read object.
        /// </returns>
        public async Task<bool> ReadBoolean(CancellationToken cancellationToken = default)
        {
            using var binaryReader = new AsyncBinaryReader(this.stream, true);
            return await binaryReader.ReadBoolAsync(cancellationToken).ConfigureAwait(false);
        }

        public async Task<int> ReadInt32(CancellationToken cancellationToken = default)
        {
            using var binaryReader = new AsyncBinaryReader(this.stream, true);
            var numBytes = await binaryReader.ReadIntAsync(cancellationToken).ConfigureAwait(false);
            if (numBytes != 4)
            {
                throw new InvalidOperationException("Expected to receive 4 bytes but got " + numBytes);
            }

            return await binaryReader.ReadIntAsync(cancellationToken).ConfigureAwait(false);
        }

        public async Task Write(bool value, CancellationToken cancellationToken = default)
        {
            using var binaryWriter = new AsyncBinaryWriter(this.stream, true);
            await binaryWriter.WriteBoolAsync(value, cancellationToken).ConfigureAwait(false);
        }

        public async Task Write(byte[] value, CancellationToken cancellationToken = default)
        {
            using var binaryWriter = new AsyncBinaryWriter(this.stream, true);
            await binaryWriter.WriteIntAsync(value.Length, cancellationToken).ConfigureAwait(false);
            await binaryWriter.WriteBytesAsync(value, cancellationToken).ConfigureAwait(false);
        }

        public async Task Write(string value, CancellationToken cancellationToken = default)
        {
            using var binaryWriter = new AsyncBinaryWriter(this.stream, true);
            var bytes = Encoding.UTF8.GetBytes(value);
            await binaryWriter.WriteIntAsync(bytes.Length, cancellationToken).ConfigureAwait(false);
            await binaryWriter.WriteBytesAsync(bytes, cancellationToken).ConfigureAwait(false);
        }

        public async Task Write(int value, CancellationToken cancellationToken = default)
        {
            using var binaryWriter = new AsyncBinaryWriter(this.stream, true);
            var bytes = BitConverter.GetBytes(value);
            await binaryWriter.WriteIntAsync(bytes.Length, cancellationToken).ConfigureAwait(false);
            await binaryWriter.WriteBytesAsync(bytes, cancellationToken).ConfigureAwait(false);
        }

        public async Task Write<T>(T value, CancellationToken cancellationToken = default)
        {
            using var binaryWriter = new AsyncBinaryWriter(this.stream, true);
            var jsonString = JsonConvert.SerializeObject(value, typeof(T), new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto });
            var jsonBytes = Encoding.UTF8.GetBytes(jsonString);

            await binaryWriter.WriteIntAsync(jsonBytes.Length, cancellationToken).ConfigureAwait(false);
            await binaryWriter.WriteBytesAsync(jsonBytes, cancellationToken).ConfigureAwait(false);
        }
    }
}
