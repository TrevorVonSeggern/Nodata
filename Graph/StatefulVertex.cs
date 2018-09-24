using System;
using Graph.Interfaces;

namespace Graph
{
    /// <summary>
    /// A class representation in a graph context.
    /// </summary>
    public enum StatefulVertexStateType
    {
        UnReached,
        Discovered,
        Identified,
    }
    public class StatefulVertex<TVertexValue>
    {

        public StatefulVertex(IVertex<TVertexValue> vertex, Type t)
        {
            Vertex = vertex;
            Type = t;
        }

        public Type Type { get; }

        public readonly IVertex<TVertexValue> Vertex;

        public StatefulVertexStateType Color = StatefulVertexStateType.UnReached;
    }
}