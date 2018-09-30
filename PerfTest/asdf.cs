using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerfTest
{
    public static class EnumerableStream
    {
        public static IEnumerable<int> SlowEnumerable()
        {
            foreach (var number in Enumerable.Range(0, 5))
            {
                Console.WriteLine("+");
                Task.Delay(TimeSpan.FromSeconds(1)).Wait();
                yield return number;
            }
        }

        public static Stream SlowStream()
        {
            return Create<int>(SlowEnumerable(), x => Encoding.Default.GetBytes(x.ToString() + Environment.NewLine).ToList());
        }

        public static EnumerableStream<T> Create<T>(IEnumerable<T> source, Func<T, List<byte>> serializer)
        {
            return new EnumerableStream<T>(source, serializer);
        }
    }

    public class EnumerableStream<T> : Stream
    {
        private readonly IEnumerator<T> _source;
        private readonly Func<T, IEnumerable<byte>> _serializer;
        private readonly Queue<byte> _buf = new Queue<byte>();

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
        public EnumerableStream(IEnumerable<T> source, Func<T, IEnumerable<byte>> serializer)
        {
            _source = source.GetEnumerator();
            _serializer = serializer;
        }

        private bool SerializeNext()
        {
            if (!_source.MoveNext())
                return false;

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

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        public override void Flush()
        {
            throw new NotSupportedException();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override long Position
        {
            get { throw new NotSupportedException(); }
            set { throw new NotSupportedException(); }
        }
    }
}