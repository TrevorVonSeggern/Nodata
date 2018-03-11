using NoData.Graph;
using NoData.Graph.Base;
using NoData.Graph.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NoData.Serialization
{
    /// <summary>
    /// Enumerates the queryable, while building a list of ignored properties.
    /// </summary>
    public class SerializeGraphTraversal
    {
        /// <summary>
        /// Warning, this will traverse every element in the enumerable.
        /// </summary>
        /// <returns></returns>
        public SerializeGraph GetSerializationSettings<TDto>(IEnumerable<TDto> query, Graph.Graph graph)
        {
            var vertices = new List<SerializableVertex>(graph.Vertices.Select(v => new SerializableVertex(v.Value.Type)));
            var edges = new List<IEdge>(
                graph.Edges.Select(e =>
                    new Edge(
                        vertices.Single(v => v.Value.Type == (e.From.Value as ItemInfo).Type),
                        vertices.Single(v => v.Value.Type == (e.From.Value as ItemInfo).Type),
                        e.Name, e.HasMany)
                        )
                    );

            //var vertices = graph.Vertices.Select(v => v as SerializableVertex).ToDictionary(x => x.Value.Type, x => x);
            //var edges = graph.Edges;

            // TODO: Add bloom filter.
            //vertices[typeof(TDto)].Traverse(query);

            return new SerializeGraph(vertices, edges);
            //return new SerializeGraph(vertices.Values, graph.Edges);
        }
    }
}
