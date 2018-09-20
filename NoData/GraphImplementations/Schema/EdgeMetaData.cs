using System;
using System.Collections.Generic;
using System.Text;
using CodeTools;

namespace NoData.GraphImplementations.Schema
{
    [Immutable]
    public class EdgeMetaData : ICloneable
    {
        public EdgeMetaData(string name, bool isCollection)
        {
            PropertyName = name;
            IsCollection = isCollection;
        }

        public string PropertyName { get; }
        public bool IsCollection { get; }

        public object Clone() => MemberwiseClone();
    }
}
