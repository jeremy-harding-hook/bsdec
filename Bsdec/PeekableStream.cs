//-----------------------------------------------------------------------
//
// Copyright 2023 Jeremy Harding Hook
//
// This file is part of Bsdec.
//
// Bsdec is free software: you can redistribute it and/or modify it under
// the terms of the GNU General Public License as published by the Free
// Software Foundation, either version 3 of the License, or (at your option)
// any later version.
//
// Bsdec is distributed in the hope that it will be useful, but WITHOUT ANY
// WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
// FOR A PARTICULAR PURPOSE. See the GNU General Public License for more
// details.
//
// You should have received a copy of the GNU General Public License along with
// Bsdec. If not, see <https://www.gnu.org/licenses/>.
//
//-----------------------------------------------------------------------

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
