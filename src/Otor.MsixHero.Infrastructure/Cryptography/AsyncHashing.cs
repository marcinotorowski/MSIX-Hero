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
        private readonly HashAlgorithm hashAlgorithm;

        public AsyncHashing(HashAlgorithm hashAlgorithm, int bufferSize = 4096)
        {
            this.hashAlgorithm = hashAlgorithm;
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
                    hashAlgorithm.TransformFinalBlock(buffer, 0, bytesRead);
                }
                else
                {
                    hashAlgorithm.TransformBlock(buffer, 0, bytesRead, buffer, 0);
                }

                var percent = (int) (100.0 * totalBytesRead / size);
                progress?.Report(new ProgressData(percent, $"Calculating hash {percent}%..."));
            } while (readAheadBytesRead != 0);

            cancellationToken.ThrowIfCancellationRequested();

            return hashAlgorithm.Hash;
        }

        public int BufferSize { get; }
    }
}
