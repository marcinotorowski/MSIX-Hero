using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace otor.msixhero.lib.Ipc.Streams
{
    /// <summary>
    /// A network stream reader.
    /// </summary>
    internal class AsyncBinaryReader : IDisposable
    {
        private readonly Stream networkStream;

        private readonly bool leaveOpen;

        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncBinaryReader" /> class.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <param name="leaveOpen">If set to <c>true</c> then leave open.</param>
        public AsyncBinaryReader(Stream stream, bool leaveOpen = false)
        {
            this.networkStream = stream;
            this.leaveOpen = leaveOpen;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if (!this.leaveOpen)
            {
                this.networkStream.Dispose();
            }
        }

        /// <summary>
        /// Reads the bytes asynchronous.
        /// </summary>
        /// <param name="bytesLength">Length of the bytes.</param>
        /// <param name="token">The token.</param>
        /// <returns>
        /// The length of bytes.
        /// </returns>
        public async Task<byte[]> ReadBytesAsync(int bytesLength, CancellationToken token = default(CancellationToken))
        {
            int bytesRead;
            var totalBytesRead = 0;
            var buffer = new byte[bytesLength];

            while (totalBytesRead < bytesLength && (bytesRead = await this.networkStream.ReadAsync(buffer, totalBytesRead, bytesLength - totalBytesRead, token).ConfigureAwait(false)) > 0)
            {
                totalBytesRead += bytesRead;
            }

            return buffer;
        }

        /// <summary>
        /// Reads the bytes asynchronous.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <returns>
        /// The length of bytes.
        /// </returns>
        public async Task<byte> ReadByteAsync(CancellationToken token = default(CancellationToken))
        {
            var result = await this.ReadBytesAsync(1, token).ConfigureAwait(false);
            return result[0];
        }

        /// <summary>
        /// Reads the integer.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <returns>
        /// The integer.
        /// </returns>
        public async Task<int> ReadIntAsync(CancellationToken token = default(CancellationToken))
        {
            var bytes = await this.ReadBytesAsync(sizeof(int), token).ConfigureAwait(false);
            return BitConverter.ToInt32(bytes, 0);
        }

        /// <summary>
        /// Reads the boolean.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <returns>
        /// The integer.
        /// </returns>
        public async Task<bool> ReadBoolAsync(CancellationToken token = default(CancellationToken))
        {
            var bytes = await this.ReadBytesAsync(sizeof(bool), token).ConfigureAwait(false);
            return BitConverter.ToBoolean(bytes, 0);
        }

        /// <summary>
        /// Reads the unsigned integer.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <returns>
        /// The unsigned integer.
        /// </returns>
        public async Task<uint> ReadUIntAsync(CancellationToken token = default(CancellationToken))
        {
            var bytes = await this.ReadBytesAsync(sizeof(uint), token).ConfigureAwait(false);
            return BitConverter.ToUInt32(bytes, 0);
        }

        /// <summary>
        /// Reads the short.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <returns>
        /// The short.
        /// </returns>
        public async Task<short> ReadShortAsync(CancellationToken token = default(CancellationToken))
        {
            var bytes = await this.ReadBytesAsync(sizeof(short), token).ConfigureAwait(false);
            return BitConverter.ToInt16(bytes, 0);
        }

        /// <summary>
        /// Reads the unsigned short.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <returns>
        /// The unsigned short.
        /// </returns>
        public async Task<ushort> ReadUShortAsync(CancellationToken token = default(CancellationToken))
        {
            var bytes = await this.ReadBytesAsync(sizeof(ushort), token).ConfigureAwait(false);
            return BitConverter.ToUInt16(bytes, 0);
        }

        /// <summary>
        /// Reads the long.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <returns>
        /// The long.
        /// </returns>
        public async Task<long> ReadLongAsync(CancellationToken token = default(CancellationToken))
        {
            var bytes = await this.ReadBytesAsync(sizeof(long), token).ConfigureAwait(false);
            return BitConverter.ToInt64(bytes, 0);
        }

        /// <summary>
        /// Reads the long.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <returns>
        /// The long.
        /// </returns>
        public async Task<ulong> ReadULongAsync(CancellationToken token = default(CancellationToken))
        {
            var bytes = await this.ReadBytesAsync(sizeof(ulong), token).ConfigureAwait(false);
            return BitConverter.ToUInt64(bytes, 0);
        }

        /// <summary>
        /// Reads the double.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <returns>
        /// The double.
        /// </returns>
        public async Task<double> ReadDoubleAsync(CancellationToken token = default(CancellationToken))
        {
            var bytes = await this.ReadBytesAsync(sizeof(double), token).ConfigureAwait(false);
            return BitConverter.ToDouble(bytes, 0);
        }

        /// <summary>
        /// Reads the string.
        /// </summary>
        /// <param name="byteCounts">The byte counts.</param>
        /// <param name="encoding">The encoding.</param>
        /// <param name="token">The token.</param>
        /// <returns>
        /// The string.
        /// </returns>
        public async Task<string> ReadStringAsync(int byteCounts, Encoding encoding, CancellationToken token = default(CancellationToken))
        {
            var bytes = await this.ReadBytesAsync(byteCounts, token).ConfigureAwait(false);
            return encoding.GetString(bytes);
        }

        /// <summary>
        /// Reads the string.
        /// </summary>
        /// <param name="byteCounts">The byte counts.</param>
        /// <param name="token">The token.</param>
        /// <returns>
        /// The string.
        /// </returns>
        public Task<string> ReadStringAsync(int byteCounts, CancellationToken token = default(CancellationToken))
        {
            return this.ReadStringAsync(byteCounts, Encoding.UTF8, token);
        }
    }

}
