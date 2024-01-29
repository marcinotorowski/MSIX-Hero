// MIT License
// 
// Copyright (C) 2024 Marcin Otorowski
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
// 
// https://github.com/marcinotorowski/simpleelevation/blob/develop/LICENSE.md

namespace Otor.MsixHero.Elevation.Ipc.Helpers
{
    public class AsyncBinaryWriter : IDisposable
    {
        private readonly Stream _stream;

        private readonly bool _leaveOpen;

        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncBinaryWriter" /> class.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <param name="leaveOpen">If set to <c>true</c> then leave open.</param>
        public AsyncBinaryWriter(Stream stream, bool leaveOpen = false)
        {
            this._stream = stream;
            this._leaveOpen = leaveOpen;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if (!this._leaveOpen)
            {
                this._stream.Dispose();
            }
        }

        /// <summary>
        /// Writes a time stamp.
        /// </summary>
        /// <param name="timeSpan">The time stamp.</param>
        /// <param name="token">The cancellation token.</param>
        /// <returns>A task that represents writing to the stream.</returns>
        public Task WriteAsync(TimeSpan timeSpan, CancellationToken token = default(CancellationToken))
        {
            return this.WriteAsync(timeSpan.Ticks, token);
        }

        /// <summary>
        /// Writes a date time.
        /// </summary>
        /// <param name="dateTime">The date time.</param>
        /// <param name="token">The cancellation token.</param>
        /// <returns>A task that represents writing to the stream.</returns>
        public Task WriteAsync(DateTime dateTime, CancellationToken token = default(CancellationToken))
        {
            return this.WriteAsync(dateTime.ToBinary(), token);
        }

        /// <summary>
        /// Writes the bytes asynchronous.
        /// </summary>
        /// <param name="bytes">The bytes.</param>
        /// <param name="token">The token.</param>
        /// <returns>
        /// The task.
        /// </returns>
        public async Task WriteAsync(byte[] bytes, CancellationToken token = default(CancellationToken))
        {
            await this._stream.WriteAsync(bytes, token).ConfigureAwait(false);
        }

        /// <summary>
        /// Writes the bytes asynchronous.
        /// </summary>
        /// <param name="b">The byte.</param>
        /// <param name="token">The token.</param>
        /// <returns>
        /// The task.
        /// </returns>
        public async Task WriteAsync(byte b, CancellationToken token = default(CancellationToken))
        {
            await _stream.WriteAsync(new [] { b }, token).ConfigureAwait(false);
        }

        /// <summary>
        /// Writes the boolean.
        /// </summary>
        /// <param name="boolean">The boolean.</param>
        /// <param name="token">The token.</param>
        /// <returns>
        /// The task.
        /// </returns>
        public Task WriteAsync(bool boolean, CancellationToken token = default(CancellationToken))
        {
            return this.WriteAsync(BitConverter.GetBytes(boolean), token);
        }

        /// <summary>
        /// Writes the integer.
        /// </summary>
        /// <param name="number">The number.</param>
        /// <param name="token">The token.</param>
        /// <returns>
        /// The task.
        /// </returns>
        public async Task WriteAsync(int number, CancellationToken token = default(CancellationToken))
        {
            await _stream.WriteAsync(BitConverter.GetBytes(number), token).ConfigureAwait(false);
        }

        /// <summary>
        /// Writes the unsigned integer.
        /// </summary>
        /// <param name="number">The number.</param>
        /// <param name="token">The token.</param>
        /// <returns>
        /// The task.
        /// </returns>
        public async Task WriteAsync(uint number, CancellationToken token = default(CancellationToken))
        {
            await _stream.WriteAsync(BitConverter.GetBytes(number), token).ConfigureAwait(false);
        }

        /// <summary>
        /// Writes the short.
        /// </summary>
        /// <param name="number">The number.</param>
        /// <param name="token">The token.</param>
        /// <returns>
        /// The task.
        /// </returns>
        public async Task WriteAsync(short number, CancellationToken token = default(CancellationToken))
        {
            await _stream.WriteAsync(BitConverter.GetBytes(number), token).ConfigureAwait(false);
        }
        
        /// <summary>
        /// Writes the unsigned short.
        /// </summary>
        /// <param name="number">The number.</param>
        /// <param name="token">The token.</param>
        /// <returns>
        /// The task.
        /// </returns>
        public async Task WriteAsync(ushort number, CancellationToken token = default(CancellationToken))
        {
            await _stream.WriteAsync(BitConverter.GetBytes(number), token).ConfigureAwait(false);
        }

        /// <summary>
        /// Writes the long.
        /// </summary>
        /// <param name="number">The number.</param>
        /// <param name="token">The token.</param>
        /// <returns>
        /// The task.
        /// </returns>
        public async Task WriteAsync(long number, CancellationToken token = default(CancellationToken))
        {
            await _stream.WriteAsync(BitConverter.GetBytes(number), token).ConfigureAwait(false);
        }

        /// <summary>
        /// Writes the long.
        /// </summary>
        /// <param name="number">The number.</param>
        /// <param name="token">The token.</param>
        /// <returns>
        /// The task.
        /// </returns>
        public async Task WriteAsync(ulong number, CancellationToken token = default(CancellationToken))
        {
            await _stream.WriteAsync(BitConverter.GetBytes(number), token).ConfigureAwait(false);
        }

        /// <summary>
        /// Writes the double.
        /// </summary>
        /// <param name="number">The number.</param>
        /// <param name="token">The token.</param>
        /// <returns>
        /// The task.
        /// </returns>
        public async Task WriteAsync(double number, CancellationToken token = default(CancellationToken))
        {
            await _stream.WriteAsync(BitConverter.GetBytes(number), token).ConfigureAwait(false);
        }
    }
}
