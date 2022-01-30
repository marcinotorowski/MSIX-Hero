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
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Otor.MsixHero.Infrastructure.Progress;

namespace Otor.MsixHero.Infrastructure.Cryptography
{
    public class AsyncHashing
    {
        private readonly HashAlgorithm _hashAlgorithm;

        public AsyncHashing(HashAlgorithm hashAlgorithm, int bufferSize = 4096)
        {
            this._hashAlgorithm = hashAlgorithm;
            this.BufferSize = bufferSize;
        }

        public async Task<byte[]> ComputeHashAsync(Stream stream, long? responseLength, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = null)
        {
            var totalBytesRead = 0L;
            var size = responseLength ?? stream.Length;
            var readAheadBuffer = new byte[this.BufferSize];
            var readAheadBytesRead = await stream.ReadAsync(readAheadBuffer, 0, readAheadBuffer.Length, cancellationToken).ConfigureAwait(false);

            totalBytesRead += readAheadBytesRead;

            do
            {
                cancellationToken.ThrowIfCancellationRequested();
                var bytesRead = readAheadBytesRead;
                var buffer = readAheadBuffer;

                readAheadBuffer = new byte[this.BufferSize];
                readAheadBytesRead = await stream.ReadAsync(readAheadBuffer, 0, readAheadBuffer.Length, cancellationToken).ConfigureAwait(false);

                totalBytesRead += readAheadBytesRead;

                if (readAheadBytesRead == 0)
                {
                    _hashAlgorithm.TransformFinalBlock(buffer, 0, bytesRead);
                }
                else
                {
                    _hashAlgorithm.TransformBlock(buffer, 0, bytesRead, buffer, 0);
                }

                var percent = (int) (100.0 * totalBytesRead / size);
                progress?.Report(new ProgressData(percent, string.Format(Resources.Localization.Infrastructure_Hashing_Format, percent)));
            } while (readAheadBytesRead != 0);

            cancellationToken.ThrowIfCancellationRequested();

            return _hashAlgorithm.Hash;
        }

        public int BufferSize { get; }
    }
}
