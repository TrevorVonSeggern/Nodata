using Immutability;
using Graph.Interfaces;

namespace Graph
{
    /// <summary>
    /// A class representation in a graph context.
    /// </summary>
    [Immutable]
    public class Vertex<TValue> : IVertex<TValue>
    {
        public TValue Value { get; }

        public Vertex(TValue value)
        {
            Value = value;
        }

        public override string ToString() => Value.ToString();

    }
}
