using System;
using System.Collections.Generic;
using System.Linq;

namespace NoData.GraphImplementations.Schema
{

    public class GraphSchema : Graph.Graph<Vertex, Edge, ClassInfo, EdgeMetaData>, ICloneable
    {
        public GraphSchema(IEnumerable<Vertex> vertices, IEnumerable<Edge> edges) : base(vertices, edges) { }

        public Vertex VertexContainingType(Type type) => Vertices.Single(v => v.Value.Type == type);

        public object Clone()
        {
            var vertices = new List<Vertex>(Vertices.Select(v => v.Clone() as Vertex));
            var edges = new List<Edge>(
                Edges.Select(e =>
                    new Edge(
                        vertices.Single(v => v.Value.Type == e.From.Value.Type),
                        vertices.Single(v => v.Value.Type == e.To.Value.Type),
                        e.Value)
                        )
                    );
            return new GraphSchema(vertices, edges);
        }

        public static GraphSchema CreateFromGeneric<TDto>()
        {
            var vertices = new List<StatefulVertex>();
            var edges = new List<Edge>();

            // add entrypoint
            vertices.Add(new StatefulVertex(new Vertex(typeof(TDto))));

            bool VerticesContainType(Type type) => vertices.Any(v => v.Vertex.Value.Type == type);
            StatefulVertex GetOrAddVertex(Type type)
            {
                StatefulVertex vertex = null;
                if (VerticesContainType(type))
                    vertex = vertices.First(v => v.Vertex.Value.Type == type);
                else
                {
                    vertex = new StatefulVertex(new Vertex(type));
                    vertices.Add(vertex);
                }
                return vertex;
            }

            while (vertices.Where(v => v.Color == StatefulVertex.StateType.Discovered).Count() != vertices.Count)
            {
                var vertex = vertices.First(v => v.Color == StatefulVertex.StateType.UnReached);
                vertex.Color = StatefulVertex.StateType.Identified;

                var classInfo = NoData.Utility.ClassInfoCache.GetOrAdd(vertex.Vertex.Value.Type);
                var tentativeConnectionToType = new List<Edge>();

                void EstablishConnection(Type connectionToType, string name, bool collection)
                {
                    var toVertex = GetOrAddVertex(connectionToType);
                    var edge = new Edge(vertex.Vertex, toVertex.Vertex, name, collection);
                    if (!edges.Contains(edge))
                        edges.Add(edge);
                }

                foreach (var childNavigationProperty in classInfo.NavigationProperties)
                {
                    var type = childNavigationProperty.PropertyType;
                    EstablishConnection(type, childNavigationProperty.Name, false);
                }
                foreach (var childCollection in classInfo.Collections)
                {
                    var type = childCollection.PropertyType.GetGenericArguments()[0];
                    EstablishConnection(type, childCollection.Name, true);
                }

                vertex.Color = StatefulVertex.StateType.Discovered;
            }

            return new GraphSchema(vertices.Select(v => v.Vertex), edges);
        }
    }
}
