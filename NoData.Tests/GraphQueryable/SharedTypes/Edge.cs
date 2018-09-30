namespace NoData.Tests.GraphQueryable.SharedTypes
{
    public class Edge : Graph.Edge<string, Vertex, string>
    {
        public Edge(Vertex from, Vertex to, string value = null) : base(from, to, value)
        {
        }
    }
}