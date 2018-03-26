using System;
using System.Collections.Generic;
using System.Text;

namespace NoData.Graph
{
    public class EdgeMetaData : ICloneable
    {
        public EdgeMetaData(string name, bool isCollection)
        {
            PropertyName = name;
            IsCollection = isCollection;
        }

        public string PropertyName { get; set; }
        public bool IsCollection { get; set; }

        public object Clone() => MemberwiseClone();
    }
}
