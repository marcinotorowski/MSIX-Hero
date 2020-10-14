using System;
using System.IO;

namespace PriFormat
{
    class SubStream : Stream
    {
        Stream baseStream;
        long subStreamPosition;
        long subStreamLength;

        public SubStream(Stream baseStream, long subStreamPosition, long subStreamLength)
        {
            this.baseStream = baseStream;
            this.subStreamPosition = subStreamPosition;
            this.subStreamLength = subStreamLength;
        }

        public long SubStreamPosition
        {
            get
            {
                return subStreamPosition;
            }
        }

        public override bool CanRead
        {
            get
            {
                return baseStream.CanRead;
            }
        }

        public override bool CanSeek
        {
            get
            {
                return baseStream.CanSeek;
            }
        }

        public override bool CanWrite
        {
            get
            {
                return false;
            }
        }

        public override long Length
        {
            get
            {
                return subStreamLength;
            }
        }

        public override long Position
        {
            get
            {
                return baseStream.Position - subStreamPosition;
            }
            set
            {
                baseStream.Position = subStreamPosition + value;
            }
        }

        public override void Flush()
        {
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (Position < 0)
                throw new InvalidOperationException("Cannot read when position is negative.");
            if (Position + count > subStreamLength)
                count = (int)(subStreamLength - Position);

            return baseStream.Read(buffer, offset, count);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            switch (origin)
            {
                case SeekOrigin.Begin:
                    return baseStream.Seek(subStreamPosition + offset, SeekOrigin.Begin) - subStreamPosition;
                case SeekOrigin.Current:
                    return baseStream.Seek(offset, SeekOrigin.Current) - subStreamPosition;
                case SeekOrigin.End:
                    return baseStream.Seek(subStreamPosition + subStreamLength + offset, SeekOrigin.Begin) - subStreamPosition;
                default:
                    throw new ArgumentException("Invalid origin.", nameof(origin));
            }
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }
    }
}
