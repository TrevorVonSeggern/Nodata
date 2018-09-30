using System;
using System.Collections.Generic;
using System.Text;

namespace Graph
{
    using System.Diagnostics.Contracts;
    using Interfaces;
    public class BloomFilter : BloomFilter<object>, IProbabilisticDataScructure { }
    public class BloomFilter<T> : IProbabilisticDataScructure<T> where T : class
    {
        private readonly int size = 100;

        public BloomFilter()
        {
            _bloomFilter = new byte[size];
        }
        public BloomFilter(int size)
        {
            this.size = size;
            _bloomFilter = new byte[size];
        }

        protected byte[] _bloomFilter;

        [Pure]
        public void AddItem(T item)
        {
            var bit = GetBit(item, out var hash);
            _bloomFilter[hash % _bloomFilter.Length] |= bit;
        }

        public bool PossiblyExists(T item)
        {
            var bit = GetBit(item, out var hash);
            return (_bloomFilter[hash % _bloomFilter.Length] & bit) != 0;
        }

        private static byte GetBit(T item, out int hash)
        {
            hash = item.GetHashCode() & 0x7FFFFFFF; // strips signed bit
            return (byte)(1 << (hash & 7)); // you have 8 bits
        }

        public object Clone()
        {
            var copy = MemberwiseClone() as BloomFilter<T>;
            copy._bloomFilter = new byte[size];
            return copy;
        }
    }
}
