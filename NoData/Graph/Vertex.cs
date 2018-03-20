using NoData.Graph.Base;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NoData.Graph
{
    /// <summary>
    /// A class representation in a graph context.
    /// </summary>
    public class Vertex : Base.Vertex, ICloneable
    {
        public new ItemInfo Value => base.Value as ItemInfo;

        public Vertex(ItemInfo value) : base(value) { }
        public Vertex(Type type) : base(new ItemInfo(type)) { }

        public void Merge(Vertex other)
        {
            if (other.Value.Type != Value.Type) throw new ArgumentException("Can't merge vertex of a different type.");
            Value.Merge(other.Value);
        }

        public new object Clone() =>new Vertex(Value.Clone() as ItemInfo);
    }

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
