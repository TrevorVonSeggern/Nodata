using System;

namespace NoData.Graph
{
    /// <summary>
    /// A class representation in a graph context.
    /// </summary>
    internal class StatefulVertex
    {
        public enum StateType
        {
            UnReached,
            Discovered,
            Identified,
        }

        public StatefulVertex(Vertex vertex)
        {
            Vertex = vertex ?? throw new ArgumentNullException(nameof(vertex));
        }

        public readonly Vertex Vertex;

        public StateType Color = StateType.UnReached;
    }
}
