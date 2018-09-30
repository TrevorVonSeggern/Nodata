using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace NoData.Utility
{
    // Thanks to https://ryandavis.io/a-lazily-evaluated-stream-wrapper-for-ienumerable/
    public static class EnumerableStream
    {
        public static EnumerableStream<T> Create<T>(IEnumerable<T> source, Func<T, List<byte>> serializer)
        {
            return new EnumerableStream<T>(source, (x) => serializer(x));
        }
        public static EnumerableStream<T> Create<T>(IEnumerable<T> source, Func<T, string> serializer, string between, string first, string last)
        {
            List<byte> Encoder(string x) => System.Text.Encoding.Default.GetBytes(x).ToList();
            return new EnumerableStream<T>(source, (x) => Encoder(serializer(x)), Encoder(between), Encoder(first), Encoder(last));
        }
    }

    public class EnumerableStream<T> : Stream
    {
        private readonly IEnumerator<T> _source;
        private readonly Func<T, IEnumerable<byte>> _serializer;
        private readonly List<byte> end;
        private readonly Queue<byte> _buf = new Queue<byte>();

        private bool firstItem = true;
        private bool _canRead = true;
        public override bool CanRead { get => _canRead; }
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length => -1;

        /// <summary>
        /// Creates a new instance of <code>EnumerableStream</code>
        /// </summary>
        /// <param name="source">The source enumerable for the EnumerableStream</param>
        /// <param name="serializer">A function that converts an instance of <code>T</code> to IEnumerable<byte></param>
        public EnumerableStream(IEnumerable<T> source, Func<T, IEnumerable<byte>> serializer, List<byte> between = null, List<byte> start = null, List<byte> end = null)
        {
            _source = source.GetEnumerator();
            _serializer = serializer;
            Between = between ?? new List<byte>();
            foreach (var b in start ?? new List<byte>())
                _buf.Enqueue(b);
            this.end = end ?? new List<byte>();
        }

        private bool SerializeNext()
        {
            if (!_source.MoveNext())
            {
                foreach (var b in end)
                    _buf.Enqueue(b);
                end.Clear();
                Between.Clear();
                return false;
            }
            else if (!firstItem)
            {
                foreach (var b in Between)
                    _buf.Enqueue(b);
            }
            else
                firstItem = false;

            foreach (var b in _serializer(_source.Current))
                _buf.Enqueue(b);

            return true;
        }

        private byte? NextByte()
        {
            if (_buf.Any() || SerializeNext())
            {
                return _buf.Dequeue();
            }
            _canRead = false;
            return null;
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
        public override void Flush() => throw new NotSupportedException();
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();

        public override long Position
        {
            get { throw new NotSupportedException(); }
            set { throw new NotSupportedException(); }
        }

        public List<byte> Between { get; }
    }
}