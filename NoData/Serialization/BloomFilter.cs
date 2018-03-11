using System;
using System.Collections.Generic;
using System.Text;

namespace NoData.Serialization
{
    public class BloomFilter
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

        private byte[] _bloomFilter;

        public void AddItem(object item)
        {
            var bit = GetBit(item, out var hash);
            _bloomFilter[hash % _bloomFilter.Length] |= bit;
        }

        public bool PossiblyExists(string item)
        {
            var bit = GetBit(item, out var hash);
            return (_bloomFilter[hash % _bloomFilter.Length] & bit) != 0;
        }

        public bool PossiblyExists(object item)
        {
            var bit = GetBit(item, out var hash);
            return _PossiblyExists(bit, hash);
        }

        private bool _PossiblyExists(byte bit, int hash) => (_bloomFilter[hash % _bloomFilter.Length] & bit) != 0;

        private static byte GetBit(string item, out int hash)
        {
            hash = Hash(item) & 0x7FFFFFFF; // strips signed bit
            return (byte)(1 << (hash & 7)); // you have 8 bits
        }

        private static byte GetBit(object item, out int hash)
        {
            hash = item.GetHashCode() & 0x7FFFFFFF; // strips signed bit
            return (byte)(1 << (hash & 7)); // you have 8 bits
        }

        private static int Hash(string item)
        {
            int result = 17;
            for (int i = 0; i < item.Length; i++)
            {
                unchecked
                {
                    result *= item[i];
                }
            }
            return result;
        }
    }
}
