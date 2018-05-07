using NoData.Graph.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NoData.Graph.Base
{
    public class Tree<TVertex, TEdge, TVertexValue, TEdgeValue> : ITree<TVertex, TEdge, TVertexValue, TEdgeValue>
        where TVertex : IVertex<TVertexValue>
        where TEdge : IEdge<TEdgeValue, TVertex, TVertexValue>
        where TVertexValue : IMergable<TVertexValue>
    {
        public virtual TVertex Root { get; protected set; }

        public virtual IEnumerable<ITuple<TEdge, ITree<TVertex, TEdge, TVertexValue, TEdgeValue>>> Children { get; protected set; }

        public Tree(TVertex root, IEnumerable<ITuple<TEdge, ITree<TVertex, TEdge, TVertexValue, TEdgeValue>>> children)
        {
            Root = root;
            Children = children;
            if (Children == null)
                return;
            //if (Children.Any(c => c.Item1.From.Value != Root))
            //    throw new ArgumentException("Children edges must all be from the root.");
        }
        
        public virtual void Traverse(Action<TEdge> callback)
        {
            if (Children == null)
                return;
            foreach(var tuple in Children)
            {
                callback(tuple.Item1);
                tuple.Item2.Traverse(callback);
            }
        }

        public virtual void Traverse(Action<TVertex> callback)
        {
            callback(Root);
            if (Children == null)
                return;
            foreach(var tuple in Children)
            {
                tuple.Item2.Traverse(callback);
            }
        }

        public virtual void Traverse(Action<TVertex, IEnumerable<TEdge>> callback)
        {
            if (Children == null)
            {
                callback(Root, new TEdge[] { });
                return;
            }
            callback(Root, Children.Select(c => c.Item1));
            foreach (var tuple in Children)
            {
                tuple.Item2.Traverse(callback);
            }
        }

        //public virtual IGraph Flatten()
        //{
        //    var edges = new List<IEdge>();
        //    Traverse(edges.Add);
        //    var vertices = new List<IVertex>();
        //    vertices.Add(root);
        //    vertices.AddRange(edges.Select(e => e.From).Where(v => v != root));
        //    vertices.AddRange(edges.Select(e => e.To));
        //    vertices = vertices.Distinct().ToList();
        //    return new Graph(vertices, edges);
        //}
    }
}
