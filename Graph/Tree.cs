using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CodeTools;
using Graph.Interfaces;

namespace Graph
{

    public class Tree<TVertex, TEdge, TVertexValue, TEdgeValue> : ITree<TVertex, TEdge, TVertexValue, TEdgeValue>
        where TVertex : class, IVertex<TVertexValue>
        where TEdge : class, IEdge<TEdgeValue, TVertex, TVertexValue>
    {
        public virtual TVertex Root { get; protected set; }

        public virtual IEnumerable<ITuple<TEdge, ITree<TVertex, TEdge, TVertexValue, TEdgeValue>>> Children { get; }

        public Tree(TVertex root, IEnumerable<ITuple<TEdge, ITree<TVertex, TEdge, TVertexValue, TEdgeValue>>> children)
        {
            Root = root;
            Children = children ?? new List<ITuple<TEdge, ITree<TVertex, TEdge, TVertexValue, TEdgeValue>>>();
            if (Children.Any(c => !root.Equals(c.Item1.From)))
                throw new ArgumentException("Children edges must all be from the root.");
        }

        public virtual void TraverseDepthFirstSearch(Action<TEdge> callback)
        {
            if (Children == null)
                return;
            foreach (var tuple in Children)
            {
                callback(tuple.Item1);
                tuple.Item2?.TraverseDepthFirstSearch(callback);
            }
        }

        public virtual void TraverseDepthFirstSearch(Action<TVertex> callback)
        {
            callback(Root);
            if (Children == null)
                return;
            foreach (var tuple in Children)
            {
                tuple.Item2?.TraverseDepthFirstSearch(callback);
            }
        }

        public virtual void TraverseDepthFirstSearch(Action<TVertex, IEnumerable<TEdge>> callback)
        {
            if (Children == null)
            {
                callback(Root, new TEdge[] { });
                return;
            }
            callback(Root, Children.Select(c => c.Item1));
            foreach (var tuple in Children)
            {
                tuple.Item2.TraverseDepthFirstSearch(callback);
            }
        }

        public IEnumerable<IPath<TEdge, TVertex, TEdgeValue, TVertexValue>> EnumerateAllPaths()
        {
            if (!Children.Any())
                yield break;
            foreach (var childPath in Children.Select(x => ITuple.Create(x.Item1, x.Item2.EnumerateAllPaths())))
            {
                var cPaths = childPath.Item2;
                var toPrepend = new List<IPath<TEdge, TVertex, TEdgeValue, TVertexValue>>();
                foreach (var cpath in cPaths)
                {
                    yield return ((cpath.PrependEdge(childPath.Item1)));
                }
                yield return new Path<TEdge, TVertex, TEdgeValue, TVertexValue>(new[] { childPath.Item1 });
            }
        }

        public Tree(TVertex root, IEnumerable<IEnumerable<TEdge>> expandPaths)
        {
            Root = root;
            var children = new List<ITuple<TEdge, Tree<TVertex, TEdge, TVertexValue, TEdgeValue>>>();

            if (expandPaths is null)
                expandPaths = new List<List<TEdge>>();

            expandPaths = expandPaths.Where(p => p != null && p.Any() == true);

            if (!expandPaths.Any())
                return;

            // Validate each path starts with root.
            if (!expandPaths.All(p => p.First().From.Equals(root)))
                throw new ArgumentException("Paths don't all begin at the same vertex");

            // each path that has the the same root.
            foreach (var path in expandPaths.GroupBy(x => x.First()))
            {
                var childPaths = path.Select(p => p.Skip(1)).Where(p => p.Any());
                var edgeToChild = path.Key;
                var childRoot = edgeToChild.To;

                children.Add(ITuple.Create(edgeToChild, new Tree<TVertex, TEdge, TVertexValue, TEdgeValue>(childRoot, childPaths)));
            }

            Children = children;
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

    // outside of class because It has to implement IMergable<TVertex>. I don't want the tree class to enforce that generic constraint.
    public static class TreeExtension
    {
        public static Graph<TVertex, TEdge, TVertexValue, TEdgeValue> Flatten<TVertex, TEdge, TVertexValue, TEdgeValue>(this Tree<TVertex, TEdge, TVertexValue, TEdgeValue> selectionTree)
                where TVertex : class, IVertex<TVertexValue>, IMergable<TVertex>
                where TEdge : class, IEdge<TEdgeValue, TVertex, TVertexValue>
        {
            return Interfaces.TreeExtension.Flatten<
                    Graph<TVertex, TEdge, TVertexValue, TEdgeValue>,
                    Tree<TVertex, TEdge, TVertexValue, TEdgeValue>,
                    TVertex,
                    TEdge,
                    TVertexValue,
                    TEdgeValue
                    >(selectionTree, (vList, eList) => new Graph<TVertex, TEdge, TVertexValue, TEdgeValue>(vList, eList));
        }
    }

}
