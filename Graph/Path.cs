using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using Graph.Interfaces;
using CodeTools;

namespace Graph
{
    [Immutable]
    public class Path<TEdge, TVertex, TEdgeValue, TVertexValue> : IPath<TEdge, TVertex, TEdgeValue, TVertexValue>, IEnumerable<TEdge>
        where TEdge : IEdge<TEdgeValue, TVertex, TVertexValue>
        where TVertex : IVertex<TVertexValue>
    {
        public virtual IEnumerable<TEdge> Edges { get; }
        private int _hashCode { get; }
        public Path(IEnumerable<TEdge> edges)
        {
            Edges = edges.ToList().AsReadOnly();
            // var toAdd = new List<TEdge>(edges.Count());
            // TEdge previousEdge = default(TEdge);
            // foreach (var edge in edges)
            // {
            //     if (ReferenceEquals(previousEdge, null))
            //         toAdd.Add(edge);
            //     else
            //     {
            //         if (!edge.From.Equals(previousEdge.To))
            //             throw new ArgumentException("IEdge list is not continuous.");
            //         else
            //             toAdd.Add(edge);
            //     }
            //     previousEdge = edge;
            // }
            // Edges = toAdd;
            _hashCode = 0;
            foreach (var edge in Edges)
                _hashCode = (_hashCode + edge.GetHashCode()) % int.MaxValue;
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
                return _hashCode == other._hashCode;
                // if (other.Edges.Count() != Edges.Count())
                //     return false;
                // if (!other.Edges.Any())
                //     return true;

                // // validate each edge
                // var oEnum = other.Edges.GetEnumerator();
                // var myEnum = Edges.GetEnumerator();
                // while (!ReferenceEquals(oEnum.Current, null) && !ReferenceEquals(myEnum.Current, null))
                // {
                //     if (!myEnum.Current.Equals(oEnum.Current))
                //         return false;
                //     oEnum.MoveNext();
                //     myEnum.MoveNext();
                // }
                // return true;
            }
            return false;
        }

        public override int GetHashCode() => this._hashCode;

        public IEnumerator<TEdge> GetEnumerator() => Edges.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => Edges.GetEnumerator();
    }
}
