using NoData.Graph.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace NoData.Graph
{
    public class ClassInfo : ICloneable, IMergable<ClassInfo>
    {
        public readonly Type Type;
        protected List<SerializeInfo> SerializeList = new List<SerializeInfo>();
        public Expression FilterExpression { get; set; }

        public ClassInfo() { }
        public ClassInfo(Type type) : this(type, new[] { new SerializeInfo() }) { }
        public ClassInfo(Type type, IEnumerable<SerializeInfo> enumerable)
        {
            Type = type;
            SerializeList.AddRange(enumerable);
        }

        public bool IsInitialized => SerializeList.Count != 1 || SerializeList[0].IsInitialized;
        public override string ToString() => Type.Name;

        internal ClassInfo Initialize(Func<IEnumerable<string>> propertyNameFunc)
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

        public object Clone() => new ClassInfo(Type, SerializeList.Select(s => (SerializeInfo)s.Clone()));

        public void Merge(ClassInfo other)
        {
            if (!IsInitialized && !other.IsInitialized)
                return;
            if (this == other)
                return;
            SerializeList.AddRange(other.SerializeList);
        }

        public override bool Equals(object obj)
        {
            if(obj is ClassInfo)
            {
                var other = obj as ClassInfo;
                return Type == other.Type;
            }
            return false;
        }
    }

}
