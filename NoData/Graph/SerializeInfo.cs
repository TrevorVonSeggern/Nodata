using NoData.Graph.Base;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NoData.Graph
{
    using ProbalisticFilterType = NoData.Graph.Base.ProbalisticCollectionFilter;

    public class SerializeInfo : ICloneable
    {
        public SerializeInfo()
        {
            filter = new ProbalisticFilterType();
            _PropertyNames = new List<string>();
            _ExpandableNames = new List<string>();
            _ClassPropertyNames = new List<string>();
            IsInitialized = false;
        }

        public SerializeInfo(IEnumerable<string> propertyNames, IEnumerable<string> expandablePropertyNames, bool isInitialized)
        {
            filter = new ProbalisticFilterType();
            _PropertyNames = new List<string>(propertyNames);
            _ExpandableNames = expandablePropertyNames;
            _ClassPropertyNames = propertyNames;
            IsInitialized = isInitialized;
        }

        public readonly bool IsInitialized;

        private readonly ICollection<string> _PropertyNames;
        private readonly IEnumerable<string> _ExpandableNames;
        private readonly IEnumerable<string> _ClassPropertyNames;
        private readonly IProbabilisticDataScructure filter;
        private bool _EmptySelectedPropertyNames = true;

        public bool DoesPropertyExist(string propName) => _PropertyNames.Contains(propName) || _ExpandableNames.Contains(propName);
        public void AddPropertyToSerialize(string propName)
        {
            if (!_ClassPropertyNames.Contains(propName)) // only add the property if it is apart of the class.
                throw new ArgumentOutOfRangeException("Property does not belong to the class.");

            if(_EmptySelectedPropertyNames) // propertyNames is cleared on the first insert of a select property
            {
                _PropertyNames.Clear();
                _EmptySelectedPropertyNames = false;
            }
            if (!_PropertyNames.Contains(propName)) // add name, and keep the _propNames distinct.
                _PropertyNames.Add(propName);
        }

        public SerializeInfo Initialize(IEnumerable<string> propertyNames) => new SerializeInfo(_PropertyNames, propertyNames, true);
        public object Clone() => new SerializeInfo(_PropertyNames, _ExpandableNames, IsInitialized);

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

        internal IEnumerable<string> PropertySelectList() => _PropertyNames;
    }
}
