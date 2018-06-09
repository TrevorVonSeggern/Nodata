using NoData.Graph.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NoData.Graph.Base
{
    public class Path<TEdge, TVertex, TEdgeValue, TVertexValue> : IPath<TEdge, TVertex, TEdgeValue, TVertexValue>
        where TEdge : IEdge<TEdgeValue, TVertex, TVertexValue>
        where TVertex : IVertex<TVertexValue>
        where TVertexValue : IMergable<TVertexValue>
    {
        public virtual IEnumerable<TEdge> Edges { get; protected set; }

        public Path(IEnumerable<TEdge> edges)
        {
            Edges = edges;
            var toAdd = new List<TEdge>(edges.Count());
            TEdge previousEdge = default(TEdge);
            foreach (var edge in edges)
            {
                if (ReferenceEquals(previousEdge, null))
                    toAdd.Add(edge);
                else
                {
                    if (!edge.From.Equals(previousEdge.To))
                        throw new ArgumentException("IEdge list is not continuous.");
                    else
                        toAdd.Add(edge);
                }
                previousEdge = edge;
            }
            Edges = toAdd;
        }

        public virtual void Traverse(Action<TEdge> edge)
        {
            foreach (var e in Edges)
                edge(e);
        }

        public override bool Equals(object obj)
        {
            if (obj is Path<TEdge, TVertex, TEdgeValue, TVertexValue>)
            {
                var other = obj as Path<TEdge, TVertex, TEdgeValue, TVertexValue>;
                if (other.Edges.Count() != Edges.Count())
                    return false;
                if (!other.Edges.Any())
                    return true;

                // validate each edge
                var oEnum = other.Edges.GetEnumerator();
                var myEnum = Edges.GetEnumerator();
                while (!ReferenceEquals(oEnum.Current, null) && !ReferenceEquals(myEnum.Current, null))
                {
                    if (!myEnum.Current.Equals(oEnum.Current))
                        return false;
                    oEnum.MoveNext();
                    myEnum.MoveNext();
                }
                return true;
            }
            return false;
        }

        public override int GetHashCode()
        {
            int result = 0;
            foreach (var edge in Edges)
                result += edge.GetHashCode() % int.MaxValue;
            return result;
        }
    }
}
