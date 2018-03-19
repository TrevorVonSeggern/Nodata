using NoData.Graph.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NoData.Graph.Base
{
    public class Tree : ITree
    {
        private readonly IVertex root;
        public IVertex Root => root;

        private readonly IEnumerable<ITuple<IEdge, ITree>> children;
        public IEnumerable<ITuple<IEdge, ITree>> Children => children;

        public Tree(IVertex root, IEnumerable<ITuple<IEdge, ITree>> children)
        {
            this.root = root;
            this.children = children;
            if (Children == null)
                return;
            if (Children.Any(c => c.Item1.From != Root))
                throw new ArgumentException("Children edges must all be from the root.");
        }

        public void Traverse(Action<IEdge> callback)
        {
            if (Children == null)
                return;
            foreach(var tuple in Children)
            {
                callback(tuple.Item1);
                tuple.Item2.Traverse(callback);
            }
        }

        public void Traverse(Action<IVertex> callback)
        {
            callback(root);
            if (Children == null)
                return;
            foreach(var tuple in Children)
            {
                tuple.Item2.Traverse(callback);
            }
        }

        public void Traverse(Action<IVertex, IEnumerable<IEdge>> callback)
        {
            if (Children == null)
            {
                callback(root, new IEdge[] { });
                return;
            }
            callback(root, Children.Select(c => c.Item1));
            foreach (var tuple in Children)
            {
                tuple.Item2.Traverse(callback);
            }
        }

        public virtual IGraph Flatten()
        {
            var edges = new List<IEdge>();
            Traverse(edges.Add);
            var vertices = new List<IVertex>();
            vertices.Add(root);
            vertices.AddRange(edges.Select(e => e.From).Where(v => v != root));
            vertices.AddRange(edges.Select(e => e.To));
            vertices = vertices.Distinct().ToList();
            return new Graph(vertices, edges);
        }
    }
}
