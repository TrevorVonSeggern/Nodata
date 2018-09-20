﻿using Graph.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using NoData.Utility;

namespace NoData.GraphImplementations.Schema
{
    public class ClassInfo : ICloneable, Graph.Interfaces.IMergable<ClassInfo>
    {
        public Type Type { get; }

        public string Name { get; }

        public IReadOnlyList<Property> Properties { get; }

        protected List<SerializeInfo> SerializeList = new List<SerializeInfo>();

        public ClassInfo(Type type) : this(type, new[] { new SerializeInfo(ClassInfoCache.GetOrAdd(type).NonExpandablePropertyNames, ClassInfoCache.GetOrAdd(type).ExpandablePropertyNames, false) }) { }
        public ClassInfo(Type type, IEnumerable<SerializeInfo> enumerable)
        {
            Type = type;
            Properties = ClassInfoCache.GetOrAdd(type).Properties.Select(x => new Property(x)).ToList();
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

        private void AssertUnInitialized()
        {
            if (SerializeList.Count != 1)
                throw new ArgumentException("Too many serialize settings to add an item to.");
        }

        public void AddItem(object t)
        {
            AssertUnInitialized();
            var info = SerializeList.Single();
            info.AddItem(t);
        }

        public bool ShouldSerializeProperty(object instance, string propertyName)
        {
            foreach (var info in SerializeList)
            {
                if (info.PossiblyExists(instance))
                {
                    if (info.DoesPropertyExist(propertyName))
                        return true;
                    return false;
                }
            }
            return false;
        }

        public object Clone() => new ClassInfo(Type, SerializeList.Select(s => (SerializeInfo)s.Clone()));

        internal void AddSelection(string propertyName)
        {
            AssertUnInitialized();
            var info = SerializeList.Single();
            info.AddPropertyToSerialize(propertyName);
        }

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
            if (obj is ClassInfo)
            {
                var other = obj as ClassInfo;
                return Type == other.Type;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return this.Type.GetHashCode();
        }

        internal IEnumerable<string> GetSelectProperties()
        {
            AssertUnInitialized();
            return SerializeList[0].PropertySelectList();
        }
    }

}