using Immutability;

namespace NoData.GraphImplementations.Schema
{
    /// <summary>
    /// A class representation in a graph context.
    /// </summary>
    [Immutable]
    public class Vertex : GraphLibrary.Vertex<ClassInfo>
    {
        public Vertex(ClassInfo value) : base(value) { }
    }
}
