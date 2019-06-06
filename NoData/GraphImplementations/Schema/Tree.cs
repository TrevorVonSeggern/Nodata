using Immutability;
using Graph;
using Graph.Interfaces;
using NoData.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace NoData.GraphImplementations.Schema
{
    [Immutable]
    public class Tree : Graph.Tree<Tree, Vertex, Edge, ClassInfo, Property>
    {
        public Tree(Vertex root) : base(root, new List<ITuple<Edge, Tree>>()) { }
        public Tree(Vertex root, IEnumerable<ITuple<Edge, Tree>> children) : base(root, children) { }

        public Tree(IEnumerable<IEnumerable<Edge>> expandPaths) : base(expandPaths, ePaths => new Tree(ePaths), v => new Tree(v))
        {
        }

        // public static Tree CreateFromPathsTree(Vertex root, IEnumerable<Path> expandPaths)
        // {
        //     var c = new List<ITuple<Edge, Tree>>();
        //     expandPaths = expandPaths?.Where(p => p != null && p.Edges?.Any() == true) ?? new List<Path>();
        //     if (expandPaths.Any())
        //     {
        //         if (!expandPaths.All(p => p.Edges.First().From.Value.TypeId == root.Value.TypeId))
        //             throw new ArgumentException("Paths don't all begin at the same vertex");

        //         foreach (var path in expandPaths.Select(p => p.Edges).GroupBy(x => x.First()))
        //         {
        //             var childPaths = path.Select(p => new Path(p.Skip(1))).Where(p => p.Edges.Any());
        //             var childRoot = path.Key.To as Vertex;

        //             var edge = new Edge(root, childRoot, path.First().First().Value);
        //             c.Add(ITuple.Create(edge, CreateFromPathsTree(childRoot, childPaths)));
        //         }
        //     }
        //     return new Tree(root, c);
        // }
    }
}
