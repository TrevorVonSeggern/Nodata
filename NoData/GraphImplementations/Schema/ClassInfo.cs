using NoData.Utility;
using Immutability;

namespace NoData.GraphImplementations.Schema
{
    [Immutable]
    public class ClassInfo
    {
        public int TypeId { get; }

        private string Name { get; }

        public IReadOnlyList<Property> Properties { get; }


        public ClassInfo(ClassInfoUtility info)
        {
            TypeId = info.Type.GetHashCode();
            Name = info.Type.Name;
            Properties = info.Properties.Select(x => new Property(x)).ToList().AsReadOnly();
        }

        public override string ToString() => $"{Name} : {TypeId}";

        // public ClassInfo(Type type, IClassCache cache) : this(type, new[] { new SerializeInfo(cache.GetOrAdd(type).NonExpandablePropertyNames, cache.GetOrAdd(type).ExpandablePropertyNames, false) }, cache) { }
        // public ClassInfo(Type type, IEnumerable<SerializeInfo> enumerable, IClassCache cache)
        // {
        //     Type = type;
        //     PropertyNames = cache.GetOrAdd(type);
        //     SerializeList.AddRange(enumerable);
        // }
        // protected List<SerializeInfo> SerializeList = new List<SerializeInfo>();

        // public bool IsInitialized => SerializeList.Count != 1 || SerializeList[0].IsInitialized;

        // internal ClassInfo Initialize(Func<IEnumerable<string>> propertyNameFunc)
        // {
        //     if (!IsInitialized)
        //         SerializeList[0] = SerializeList[0].Initialize(propertyNameFunc());
        //     return this;
        // }

        // private void AssertUnInitialized()
        // {
        //     if (SerializeList.Count != 1)
        //         throw new ArgumentException("Too many serialize settings to add an item to.");
        // }

        // public void AddItem(object t)
        // {
        //     AssertUnInitialized();
        //     var info = SerializeList.Single();
        //     info.AddItem(t);
        // }

        // public bool ShouldSerializeProperty(object instance, string propertyName)
        // {
        //     foreach (var info in SerializeList)
        //     {
        //         if (info.PossiblyExists(instance))
        //         {
        //             if (info.DoesPropertyExist(propertyName))
        //                 return true;
        //             return false;
        //         }
        //     }
        //     return false;
        // }

        // // public object Clone() => new ClassInfo(Type, SerializeList.Select(s => (SerializeInfo)s.Clone()));

        // internal void AddSelection(string propertyName)
        // {
        //     AssertUnInitialized();
        //     var info = SerializeList.Single();
        //     info.AddPropertyToSerialize(propertyName);
        // }

        // public void Merge(ClassInfo other)
        // {
        //     if (!IsInitialized && !other.IsInitialized)
        //         return;
        //     if (this == other)
        //         return;
        //     SerializeList.AddRange(other.SerializeList);
        // }

        public override bool Equals(object obj)
        {
            if (obj is ClassInfo)
            {
                var other = obj as ClassInfo;
				if(other is null)
					return false;
                return TypeId == other.TypeId;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return this.TypeId.GetHashCode();
        }

        // internal IEnumerable<string> GetSelectProperties()
        // {
        //     AssertUnInitialized();
        //     return SerializeList[0].PropertySelectList();
        // }
    }

}
