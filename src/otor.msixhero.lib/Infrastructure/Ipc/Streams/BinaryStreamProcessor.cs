using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace otor.msixhero.lib.Infrastructure.Ipc.Streams
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
            using (var binaryReader = new AsyncBinaryReader(this.stream, true))
            {
                var msgLength = await binaryReader.ReadIntAsync(cancellationToken).ConfigureAwait(false);
                if (msgLength == 0)
                {
                    return null;
                }

                var bytes = await binaryReader.ReadBytesAsync(msgLength, cancellationToken).ConfigureAwait(false);
                return System.Text.Encoding.UTF8.GetString(bytes);
            }
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
            using (var binaryReader = new AsyncBinaryReader(this.stream, true))
            {
                var msgLength = await binaryReader.ReadIntAsync(cancellationToken).ConfigureAwait(false);
                if (msgLength == 0)
                {
                    return null;
                }

                var bytes = await binaryReader.ReadBytesAsync(msgLength, cancellationToken).ConfigureAwait(false);
                return bytes;
            }
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
            using (var binaryReader = new AsyncBinaryReader(this.stream, true))
            {
                var msgLength = await binaryReader.ReadIntAsync(cancellationToken).ConfigureAwait(false);
                if (msgLength == 0)
                {
                    return default(T);
                }

                var bytes = await binaryReader.ReadBytesAsync(msgLength, cancellationToken).ConfigureAwait(false);

                var xmlSerializer = new XmlSerializer(typeof(T));
                using (var reader = new MemoryStream(bytes))
                {
                    return (T)xmlSerializer.Deserialize(reader);
                }
            }
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
            using (var binaryReader = new AsyncBinaryReader(this.stream, true))
            {
                return await binaryReader.ReadBoolAsync(cancellationToken).ConfigureAwait(false);
            }
        }

        public async Task<int> ReadInt32(CancellationToken cancellationToken = default)
        {
            using (var binaryReader = new AsyncBinaryReader(this.stream, true))
            {
                var numBytes = await binaryReader.ReadIntAsync(cancellationToken).ConfigureAwait(false);
                if (numBytes != 4)
                {
                    throw new InvalidOperationException("Expected to receive 4 bytes but got " + numBytes);
                }

                return await binaryReader.ReadIntAsync(cancellationToken).ConfigureAwait(false);
            }
        }

        public async Task Write(bool value, CancellationToken cancellationToken = default)
        {
            using (var binaryWriter = new AsyncBinaryWriter(this.stream, true))
            {
                await binaryWriter.WriteBoolAsync(value, cancellationToken).ConfigureAwait(false);
            }
        }

        public async Task Write(byte[] value, CancellationToken cancellationToken = default)
        {
            using (var binaryWriter = new AsyncBinaryWriter(this.stream, true))
            {
                await binaryWriter.WriteIntAsync(value.Length, cancellationToken).ConfigureAwait(false);
                await binaryWriter.WriteBytesAsync(value, cancellationToken).ConfigureAwait(false);
            }
        }

        public async Task Write(string value, CancellationToken cancellationToken = default)
        {
            using (var binaryWriter = new AsyncBinaryWriter(this.stream, true))
            {
                var bytes = System.Text.Encoding.UTF8.GetBytes(value);
                await binaryWriter.WriteIntAsync(bytes.Length, cancellationToken).ConfigureAwait(false);
                await binaryWriter.WriteBytesAsync(bytes, cancellationToken).ConfigureAwait(false);
            }
        }

        public async Task Write(int value, CancellationToken cancellationToken = default)
        {
            using (var binaryWriter = new AsyncBinaryWriter(this.stream, true))
            {
                var bytes = BitConverter.GetBytes(value);
                await binaryWriter.WriteIntAsync(bytes.Length, cancellationToken).ConfigureAwait(false);
                await binaryWriter.WriteBytesAsync(bytes, cancellationToken).ConfigureAwait(false);
            }
        }

        public async Task Write<T>(T value, CancellationToken cancellationToken = default)
        {
            using (var binaryWriter = new AsyncBinaryWriter(this.stream, true))
            {
                using (var memoryArray = new MemoryStream())
                {
                    var serializer = new XmlSerializer(typeof(T));
                    serializer.Serialize(memoryArray, value);
                    memoryArray.Seek(0, SeekOrigin.Begin);
                    
                    var bytes = memoryArray.ToArray();
                    await binaryWriter.WriteIntAsync(bytes.Length, cancellationToken).ConfigureAwait(false);
                    await binaryWriter.WriteBytesAsync(bytes, cancellationToken).ConfigureAwait(false);
                }
            }
        }
    }
}
