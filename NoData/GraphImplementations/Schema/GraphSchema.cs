using Immutability;
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
            public static GraphSchema Graph { get; } = CreateFromGeneric<TDto>(CacheInstance);
        }
        public static readonly IClassCache CacheInstance = new ClassCache();

        private static GraphSchema CreateFromGeneric<TDto>(IClassCache cache)
        {
            var vertices = new List<StatefulVertex<ClassInfo>>();
            var edges = new List<Edge>();
            var types = new HashSet<Type>();

            // Helper funcs
            // get the class info from type via the cache.
            ClassInfoUtility GetClassInfoFromType(Type t)
            {
                var hash = t.GetHashCode();
                if (cache.HasKey(hash))
                    return cache.Get(hash);
                return cache.GetOrAdd(t);
            }

            bool VerticesContainType(Type type) => vertices.Any(v => v.Vertex.Value.TypeId == type.GetHashCode());
            StatefulVertex<ClassInfo> GetOrAddVertex(Type type)
            {
                StatefulVertex<ClassInfo>? vertex = null;
                if (VerticesContainType(type))
                    vertex = vertices.First(v => v.Vertex.Value.TypeId == type.GetHashCode());
                else
                {
                    vertex = new StatefulVertex<ClassInfo>(new Vertex(new ClassInfo(GetClassInfoFromType(type))), type);
                    vertices.Add(vertex);
                    types.Add(type);
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
                    var edge = new Edge((vertex.Vertex as Vertex)!, (toVertex.Vertex as Vertex)!, property);
                    if (!edges.Contains(edge))
                        edges.Add(edge);
                }

                foreach (var childNavigationProperty in vertex.Vertex.Value.Properties.Where(p => p.IsNavigationProperty))
                {
                    var type = classInfo.NavigationProperties.FirstOrDefault(x => x.Name == childNavigationProperty.Name)?.PropertyType;
                    if (type is null) continue;
                    EstablishConnection(type, childNavigationProperty);
                }
                foreach (var childCollection in vertex.Vertex.Value.Properties.Where(p => p.IsCollection))
                {
                    var type = classInfo.Collections.FirstOrDefault(x => x.Name == childCollection.Name)?.PropertyType.GetGenericArguments()[0];
                    if (type is null) continue;
                    EstablishConnection(type, childCollection);
                }

                vertex.Color = StatefulVertexStateType.Discovered;
            }

            return new GraphSchema(vertices?.Select(v => (v.Vertex as Vertex)!) ?? new List<Vertex>().AsEnumerable(), edges);
        }
    }
}
