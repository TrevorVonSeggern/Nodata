using NoData.Graph;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NoData.Serialization
{
    public class SerializableVertex : Vertex
    {
        public SerializableVertex(Type type) : base(type)
        {
        }

        public void Traverse(IEnumerable<object> item)
        {

        }

        public void TraverseSingle(IEnumerable<object> item)
        {

        }

        private BloomFilter bloomFilter = new BloomFilter();

        public void AddItem(object t)
        {
            if (Value.Type.IsAssignableFrom(t.GetType()))
                bloomFilter.AddItem(t.GetHashCode());
            else
                throw new ArgumentException("Could not add item to serializable vertex.");
        }

        public bool ItemPossiblyExists(object t) => bloomFilter.PossiblyExists(t);
    }
}
