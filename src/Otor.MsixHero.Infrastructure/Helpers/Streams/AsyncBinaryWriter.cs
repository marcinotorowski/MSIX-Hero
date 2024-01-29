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

using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Otor.MsixHero.Infrastructure.Helpers.Streams
{
    public class AsyncBinaryWriter : IDisposable
    {
        private readonly Stream stream;

        private readonly bool leaveOpen;

        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncBinaryWriter" /> class.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <param name="leaveOpen">If set to <c>true</c> then leave open.</param>
        public AsyncBinaryWriter(Stream stream, bool leaveOpen = false)
        {
            this.stream = stream;
            this.leaveOpen = leaveOpen;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if (!this.leaveOpen)
            {
                this.stream.Dispose();
            }
        }

        /// <summary>
        /// Writes the bytes asynchronous.
        /// </summary>
        /// <param name="bytes">The bytes.</param>
        /// <param name="token">The token.</param>
        /// <returns>
        /// The task.
        /// </returns>
        public Task WriteBytesAsync(byte[] bytes, CancellationToken token = default(CancellationToken))
        {
            return this.stream.WriteAsync(bytes, 0, bytes.Length, token);
        }

        /// <summary>
        /// Writes the bytes asynchronous.
        /// </summary>
        /// <param name="b">The byte.</param>
        /// <param name="token">The token.</param>
        /// <returns>
        /// The task.
        /// </returns>
        public Task WriteByteAsync(byte b, CancellationToken token = default(CancellationToken))
        {
            return this.stream.WriteAsync(new[] { b }, 0, 1, token);
        }

        /// <summary>
        /// Writes the boolean.
        /// </summary>
        /// <param name="boolean">The boolean.</param>
        /// <param name="token">The token.</param>
        /// <returns>
        /// The task.
        /// </returns>
        public Task WriteBoolAsync(bool boolean, CancellationToken token = default(CancellationToken))
        {
            return this.WriteBytesAsync(BitConverter.GetBytes(boolean), token);
        }

        /// <summary>
        /// Writes the integer.
        /// </summary>
        /// <param name="number">The number.</param>
        /// <param name="token">The token.</param>
        /// <returns>
        /// The task.
        /// </returns>
        public Task WriteIntAsync(int number, CancellationToken token = default(CancellationToken))
        {
            return this.WriteBytesAsync(BitConverter.GetBytes(number), token);
        }

        /// <summary>
        /// Writes the unsigned integer.
        /// </summary>
        /// <param name="number">The number.</param>
        /// <param name="token">The token.</param>
        /// <returns>
        /// The task.
        /// </returns>
        public Task WriteUIntAsync(uint number, CancellationToken token = default(CancellationToken))
        {
            return this.WriteBytesAsync(BitConverter.GetBytes(number), token);
        }

        /// <summary>
        /// Writes the short.
        /// </summary>
        /// <param name="number">The number.</param>
        /// <param name="token">The token.</param>
        /// <returns>
        /// The task.
        /// </returns>
        public Task WriteShortAsync(short number, CancellationToken token = default(CancellationToken))
        {
            return this.WriteBytesAsync(BitConverter.GetBytes(number), token);
        }

        /// <summary>
        /// Writes the short.
        /// </summary>
        /// <param name="streamToWrite">The stream.</param>
        /// <param name="buffer">The buffer.</param>
        /// <param name="token">The token.</param>
        /// <returns>
        /// The task.
        /// </returns>
        public Task WriteStreamAsync(Stream streamToWrite, int buffer, CancellationToken token = default(CancellationToken))
        {
            return streamToWrite.CopyToAsync(this.stream, buffer, token);
        }

        /// <summary>
        /// Writes the unsigned short.
        /// </summary>
        /// <param name="number">The number.</param>
        /// <param name="token">The token.</param>
        /// <returns>
        /// The task.
        /// </returns>
        public Task WriteUShortAsync(ushort number, CancellationToken token = default(CancellationToken))
        {
            return this.WriteBytesAsync(BitConverter.GetBytes(number), token);
        }

        /// <summary>
        /// Writes the long.
        /// </summary>
        /// <param name="number">The number.</param>
        /// <param name="token">The token.</param>
        /// <returns>
        /// The task.
        /// </returns>
        public Task WriteLongAsync(long number, CancellationToken token = default(CancellationToken))
        {
            return this.WriteBytesAsync(BitConverter.GetBytes(number), token);
        }

        /// <summary>
        /// Writes the long.
        /// </summary>
        /// <param name="number">The number.</param>
        /// <param name="token">The token.</param>
        /// <returns>
        /// The task.
        /// </returns>
        public Task WriteULongAsync(ulong number, CancellationToken token = default(CancellationToken))
        {
            return this.WriteBytesAsync(BitConverter.GetBytes(number), token);
        }

        /// <summary>
        /// Writes the double.
        /// </summary>
        /// <param name="number">The number.</param>
        /// <param name="token">The token.</param>
        /// <returns>
        /// The task.
        /// </returns>
        public Task WriteDoubleAsync(double number, CancellationToken token = default(CancellationToken))
        {
            return this.WriteBytesAsync(BitConverter.GetBytes(number), token);
        }

        /// <summary>
        /// Writes the string.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="encoding">The encoding.</param>
        /// <param name="token">The token.</param>
        /// <returns>
        /// The task.
        /// </returns>
        public Task WriteStringAsync(string text, Encoding encoding, CancellationToken token = default(CancellationToken))
        {
            return this.WriteBytesAsync(encoding.GetBytes(text), token);
        }

        /// <summary>
        /// Writes the string.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="token">The token.</param>
        /// <returns>
        /// The task.
        /// </returns>
        public Task WriteStringAsync(string text, CancellationToken token = default(CancellationToken))
        {
            return this.WriteStringAsync(text, Encoding.UTF8, token);
        }
    }
}
