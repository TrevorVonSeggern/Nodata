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
        private readonly TVertex root;
        public TVertex Root => root;

        private readonly IEnumerable<ITuple<TEdge, ITree<TVertex, TEdge, TVertexValue, TEdgeValue>>> children;
        public IEnumerable<ITuple<TEdge, ITree<TVertex, TEdge, TVertexValue, TEdgeValue>>> Children => children;

        public Tree(TVertex root, IEnumerable<ITuple<TEdge, ITree<TVertex, TEdge, TVertexValue, TEdgeValue>>> children)
        {
            this.root = root;
            this.children = children;
            if (Children == null)
                return;
            //if (Children.Any(c => c.Item1.From.Value != Root))
            //    throw new ArgumentException("Children edges must all be from the root.");
        }
        
        public void Traverse(Action<TEdge> callback)
        {
            if (Children == null)
                return;
            foreach(var tuple in Children)
            {
                callback(tuple.Item1);
                tuple.Item2.Traverse(callback);
            }
        }

        public void Traverse(Action<TVertex> callback)
        {
            callback(root);
            if (Children == null)
                return;
            foreach(var tuple in Children)
            {
                tuple.Item2.Traverse(callback);
            }
        }

        public void Traverse(Action<TVertex, IEnumerable<TEdge>> callback)
        {
            if (Children == null)
            {
                callback(root, new TEdge[] { });
                return;
            }
            callback(root, Children.Select(c => c.Item1));
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
