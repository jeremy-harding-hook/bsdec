using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bsdec
{
    internal class PeekableStream : Stream
    {
        private readonly Stream underlyingStream;
        private readonly Queue<int> peekBuffer = new();

        public PeekableStream(Stream stream)
        {
            underlyingStream = stream;
        }

        public int Peek()
        {
            int value = underlyingStream.ReadByte();
            peekBuffer.Enqueue(value);
            return value;
        }

        public override bool CanRead => underlyingStream.CanRead;

        public override bool CanSeek => underlyingStream.CanSeek;

        public override bool CanWrite => underlyingStream.CanWrite;

        public override long Length => peekBuffer.Count + underlyingStream.Length;

        private long position;
        public override long Position
        {
            get => position;
            set
            {
                if (CanSeek)
                    throw new NotSupportedException("Stream is not seekable");
                if (value < 0)
                    throw new ArgumentOutOfRangeException(nameof(value), "Position cannot be less than 0.");
                if (value >= underlyingStream.Length)
                    throw new ArgumentOutOfRangeException(nameof(value), "Position cannot be less than the stream length.");
                position = value;
                underlyingStream.Position = position;
                peekBuffer.Clear();
            }
        }

        public override void Flush()
        {
            underlyingStream.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            ValidateBufferArguments(buffer, offset, count);

            int bytesRead = 0;

            while (peekBuffer.Any() && count-- > 0) {
                int temp = peekBuffer.Dequeue();
                if (temp == -1)
                    return bytesRead;
                buffer[offset++] = (byte)temp;
                bytesRead++;
            }

            if(count > 0)
            {
                bytesRead += underlyingStream.Read(buffer, offset, count);
            }

            return bytesRead;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            switch (origin)
            {
                case SeekOrigin.Begin:
                    Position = offset;
                    break;
                case SeekOrigin.Current:
                    Position += offset;
                    break;
                case SeekOrigin.End:
                    Position = Length + offset;
                    break;
            }
            return position;
        }

        public override void SetLength(long value)
        {
            underlyingStream.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            underlyingStream.Write(buffer, offset, count);
        }
    }
}
