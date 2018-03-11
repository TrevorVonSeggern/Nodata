using NoData.Graph;
using NoData.Graph.Base;
using NoData.Graph.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NoData.Serialization
{
    public class SerializeGraph : Graph.Graph
    {
        public new IEnumerable<SerializableVertex> Vertices => base.Vertices.Cast<SerializableVertex>();

        public SerializeGraph(IEnumerable<SerializableVertex> vertices, IEnumerable<IEdge> edges) : base(vertices, edges)
        {

        }

        public bool ShouldSerializeProperty(Type declaringType, string propertyName, Type propertyType)
        {
            var vertex = Vertices.Single(v => v.Value.Type == declaringType);
            if (vertex.Value.PropertyNames.Contains(propertyName) || Edges.Any(e => e.From == vertex && e.Name == propertyName))
            {
                return true;
            }
            return false;
        }
    }
}
