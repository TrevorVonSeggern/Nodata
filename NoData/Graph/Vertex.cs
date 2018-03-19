using NoData.Graph.Base;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NoData.Graph
{
    public struct SerializeInfo : ICloneable
    {
        public List<string> PropertyNames { get; set; }
        private IProbabilisticDataScructure filter { get; set; }
        public bool IsInitialized { get; private set; }

        public SerializeInfo Initialize(IEnumerable<string> propertyNames)
        {
            filter = new ProbalisticCollectionFilter();
            PropertyNames = new List<string>(propertyNames);
            IsInitialized = true;
            return this;
        }

        public bool PossiblyExists(object t) => filter == null ? false : filter.PossiblyExists(t);
        public void AddItem(object t)
        {
            if (filter != null) filter.AddItem(t);
        }
        public void AddItem(object t, Type Type)
        {
            if (Type.IsAssignableFrom(t.GetType()))
                AddItem(t);
            else
                throw new ArgumentException("Could not add item to serializable vertex.");
        }

        public object Clone()
        {
            return new SerializeInfo
            {
                PropertyNames = PropertyNames == null ? null : new List<string>(PropertyNames),
                filter = filter == null ? null : filter.Clone() as IProbabilisticDataScructure,
                IsInitialized = IsInitialized,
            };
        }
    }

    public class ItemInfo : ICloneable
    {
        public ItemInfo() { }
        public ItemInfo(Type type)
        {
            PropertyNames = Utility.ClassInfoCache.GetOrAdd(type).NonExpandablePropertyNames.ToList();
            Type = type;
            var unInitializedInfo = new SerializeInfo();
            SerializeList.Add(unInitializedInfo);
        }

        public Type Type;
        public IList<string> PropertyNames;
        protected List<SerializeInfo> SerializeList = new List<SerializeInfo>();

        public bool IsInitialized => SerializeList.Count != 1 || SerializeList[0].IsInitialized;
        public override string ToString() => Type.Name;

        internal ItemInfo Initialize(Func<IEnumerable<string>> propertyNameFunc)
        {
            if (!IsInitialized)
                SerializeList[0] = SerializeList[0].Initialize(propertyNameFunc());
            return this;
        }

        public void AddItem(object t)
        {
            if (SerializeList.Count() != 1)
                throw new ArgumentException("Too many serialize settings to add an item to.");
            var info = SerializeList.Single();
            info.AddItem(t);
        }

        public void Merge(ItemInfo other)
        {
            if (!IsInitialized && !other.IsInitialized)
                return;
            if (this == other)
                return;
            SerializeList.AddRange(other.SerializeList);
        }

        public bool ShouldSerializeProperty(object instance, string propertyName)
        {
            foreach(var info in SerializeList)
            {
                if (info.PossiblyExists(instance))
                {
                    if (info.PropertyNames.Contains(propertyName))
                        return true;
                    return false;
                }
            }
            return false;
        }

        public object Clone()
        {
            return new ItemInfo()
            {
                Type = Type,
                PropertyNames = new List<string>(PropertyNames),
                SerializeList = SerializeList.Select(s => (SerializeInfo)s.Clone()).ToList(),
            };
        }
    }

    /// <summary>
    /// A class representation in a graph context.
    /// </summary>
    public class Vertex : Base.Vertex, ICloneable
    {
        public new ItemInfo Value => base.Value as ItemInfo;

        public Vertex(ItemInfo value) : base(value) { }
        public Vertex(Type type) : base(new ItemInfo(type)) { }

        public void Merge(Vertex other)
        {
            if (other.Value.Type != Value.Type)
                throw new ArgumentException("Can't merge vertex of a different type.");
            Value.Merge(other.Value);
        }

        public new object Clone() =>new Vertex(Value.Clone() as ItemInfo);
    }

    /// <summary>
    /// A class representation in a graph context.
    /// </summary>
    internal class StatefulVertex
    {
        public enum StateType
        {
            UnReached,
            Discovered,
            Identified,
        }

        public StatefulVertex(Vertex vertex)
        {
            Vertex = vertex ?? throw new ArgumentNullException(nameof(vertex));
        }

        public readonly Vertex Vertex;

        public StateType Color = StateType.UnReached;
    }
}
