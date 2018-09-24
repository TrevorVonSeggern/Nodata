using System;
using System.Collections.Generic;
using System.Linq;
using CodeTools;
using Graph;
using NoData.Utility;

namespace NoData.GraphImplementations.Schema
{
    [Immutable]
    public class GraphSchema : Graph.Graph<Vertex, Edge, ClassInfo, Property>
    {
        public GraphSchema(IEnumerable<Vertex> vertices, IEnumerable<Edge> edges) : base(vertices, edges) { }

        public Vertex VertexContainingType(Type type) => Vertices.First(v => v.Value.TypeId == type.GetHashCode());
        public Vertex VertexContainingTypeId(int typeId) => Vertices.First(v => v.Value.TypeId == typeId);

        public static class Cache<TDto>
        {
            public static readonly GraphSchema Graph = CreateFromGeneric<TDto>(new ClassCache());
        }

        private static GraphSchema CreateFromGeneric<TDto>(IClassCache cache)
        {
            var vertices = new List<StatefulVertex<ClassInfo>>();
            var edges = new List<Edge>();

            // Helper funcs
            // get the class info from type via the cache.
            ClassInfoUtility GetClassInfoFromType(Type t) => cache.GetOrAdd(t);

            bool VerticesContainType(Type type) => vertices.Any(v => v.Vertex.Value.TypeId == type.GetHashCode());
            StatefulVertex<ClassInfo> GetOrAddVertex(Type type)
            {
                StatefulVertex<ClassInfo> vertex = null;
                if (VerticesContainType(type))
                    vertex = vertices.First(v => v.Vertex.Value.TypeId == type.GetHashCode());
                else
                {
                    vertex = new StatefulVertex<ClassInfo>(new Vertex(new ClassInfo(GetClassInfoFromType(type))), type);
                    vertices.Add(vertex);
                }
                return vertex;
            }
            // initialize root.
            GetOrAddVertex(typeof(TDto));

            while (vertices.Any(v => v.Color != StatefulVertexStateType.Discovered))
            {
                var vertex = vertices.First(v => v.Color == StatefulVertexStateType.UnReached);
                vertex.Color = StatefulVertexStateType.Identified;

                var classInfo = GetClassInfoFromType(vertex.Type);
                var tentativeConnectionToType = new List<Edge>();

                void EstablishConnection(Type connectionToType, Property property)
                {
                    var toVertex = GetOrAddVertex(connectionToType);
                    var edge = new Edge(vertex.Vertex as Vertex, toVertex.Vertex as Vertex, property);
                    if (!edges.Contains(edge))
                        edges.Add(edge);
                }

                foreach (var childNavigationProperty in vertex.Vertex.Value.Properties.Where(p => p.IsNavigationProperty))
                {
                    var type = classInfo.NavigationProperties.First(x => x.Name == childNavigationProperty.Name).PropertyType;
                    EstablishConnection(type, childNavigationProperty);
                }
                foreach (var childCollection in vertex.Vertex.Value.Properties.Where(p => p.IsCollection))
                {
                    var type = classInfo.Collections.First(x => x.Name == childCollection.Name).PropertyType.GetGenericArguments()[0];
                    EstablishConnection(type, childCollection);
                }

                vertex.Color = StatefulVertexStateType.Discovered;
            }

            return new GraphSchema(vertices.Select(v => v.Vertex as Vertex), edges);
        }
    }
}
