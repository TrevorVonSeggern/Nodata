using System;
using System.Collections.Generic;
using System.Linq;

namespace NoData.Graph
{
    public class ItemInfo : ICloneable
    {
        public readonly Type Type;
        protected List<SerializeInfo> SerializeList = new List<SerializeInfo>();

        public ItemInfo() { }
        public ItemInfo(Type type) : this(type, new[] { new SerializeInfo() }) { }
        public ItemInfo(Type type, IEnumerable<SerializeInfo> enumerable)
        {
            //PropertyNames = Utility.ClassInfoCache.GetOrAdd(type).NonExpandablePropertyNames.ToList();
            Type = type;
            SerializeList.AddRange(enumerable);
        }

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
            foreach (var info in SerializeList)
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

        public object Clone() => new ItemInfo(Type, SerializeList.Select(s => (SerializeInfo)s.Clone()));
    }

}
