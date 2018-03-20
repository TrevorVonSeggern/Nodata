using NoData.Graph.Base;
using System;
using System.Collections.Generic;
using System.Text;

namespace NoData.Graph
{
    using ProbalisticFilterType = NoData.Graph.Base.ProbalisticCollectionFilter;

    public class SerializeInfo : ICloneable
    {
        public SerializeInfo()
        {
            filter = new ProbalisticFilterType();
            PropertyNames = new List<string>();
            IsInitialized = false;
        }

        public SerializeInfo(IEnumerable<string> propertyNames, bool isInitialized)
        {
            filter = new ProbalisticFilterType();
            PropertyNames = new List<string>(propertyNames);
            IsInitialized = isInitialized;
        }

        public readonly IEnumerable<string> PropertyNames;
        private readonly IProbabilisticDataScructure filter;
        public readonly bool IsInitialized;

        public SerializeInfo Initialize(IEnumerable<string> propertyNames) => new SerializeInfo(propertyNames, true);
        public object Clone() => new SerializeInfo(PropertyNames, IsInitialized);

        public bool PossiblyExists(object t) => filter == null ? false : filter.PossiblyExists(t);
        public void AddItem(object t)
        {
            if (filter == null)
                throw new ArgumentNullException("Un-initialized filter. Can't add item to bloom filter.");
            filter.AddItem(t);
        }
        public void AddItem(object t, Type Type)
        {
            if (Type.IsAssignableFrom(t.GetType()))
                AddItem(t);
            else
                throw new ArgumentException("Could not add item to serializable vertex.");
        }
    }
}
