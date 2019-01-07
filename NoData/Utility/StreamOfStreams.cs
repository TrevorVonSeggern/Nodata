using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace NoData.Utility
{
    public class StreamOfStreams : Stream
    {
        private List<Stream> streams { get; }

        private bool _canRead = true;
        public override bool CanRead => _canRead;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length => streams.Select(x => x.Length).Sum();
        private long position { get; set; }
        private int currentStreamIndex { get; set; } = 0;

        public StreamOfStreams(IEnumerable<Stream> streams)
        {
            this.streams = streams.ToList();
        }
        
        private Stream CurrentStream()
        {
            if (streams.Count <= currentStreamIndex)
                return null;
            var stream = streams[currentStreamIndex];
            if (stream.CanRead && stream.Position != stream.Length)
                return streams[currentStreamIndex];

            ++currentStreamIndex;
            return CurrentStream();
        }

        private byte? NextByte()
        {
            var stream = CurrentStream();
            if (stream is null)
                return null;

            var read = stream.ReadByte();
            if (read == -1)
            {
                if (stream != CurrentStream() || streams.Count < currentStreamIndex)
                {
                    ++currentStreamIndex;
                    return NextByte();
                }
                _canRead = false;
                return null;
            }
            return (byte)read;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            var read = 0;
            while (read < count)
            {
                var mayb = NextByte();
                if (mayb == null) break;

                buffer[offset + read] = (byte)mayb;
                read++;
            }

            return read;
        }

        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
        public override void Flush() { }
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();

        public override long Position
        {
            get { return position; }
            set { throw new NotSupportedException(); }
        }
    }
}