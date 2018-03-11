using System;
using System.Collections.Generic;
using System.Linq;

namespace NoData.Graph
{
    using Interfaces;
    using NoData.Graph.Base;

    public class Graph : Base.Graph
    {
        public new IEnumerable<Vertex> Vertices => base.Vertices.Cast<Vertex>();

        public Graph() : base() { }

        public Graph(IEnumerable<Vertex> vertices, IEnumerable<IEdge> edges, bool verticesUnique = true, bool edgesUnique = true, bool danglingEdges = false)
            : base(vertices, edges, verticesUnique, edgesUnique, danglingEdges)
        { }

        public Graph(IEnumerable<Vertex> vertices, IEnumerable<IEdge> edges) : base(vertices, edges)
        {
        }

        public static Graph CreateFromGeneric<TDto>()
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

            while(vertices.Where(v => v.Color == StatefulVertex.StateType.Discovered).Count() != vertices.Count())
            {
                var vertex = vertices.First(v => v.Color == StatefulVertex.StateType.UnReached);
                vertex.Color = StatefulVertex.StateType.Identified;

                var classInfo = Utility.ClassInfoCache.GetOrAdd(vertex.Vertex.Value.Type);
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
                foreach(var childCollection in classInfo.Collections)
                {
                    var type = childCollection.PropertyType.GetGenericArguments()[0];
                    EstablishConnection(type, childCollection.Name, true);
                }

                vertex.Color = StatefulVertex.StateType.Discovered;
            }

            return new Graph(vertices.Select(v => v.Vertex), edges);
        }
    }
}
