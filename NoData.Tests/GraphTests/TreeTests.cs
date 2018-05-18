using NoData.Graph.Interfaces;
using NoData.Graph.Utility;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using NoData.Tests.SharedExampleClasses;

namespace NoData.Tests.GraphTests
{
    using NoData.Graph;

    [TestFixture]
    public class TreeTests
    {
        [Test]
        public void Tree_Traverse_Success()
        {
            var graph = Graph.CreateFromGeneric<Dto>();
            var root = graph.VertexContainingType(typeof(Dto));
            var childVertex = graph.VertexContainingType(typeof(DtoChild));

            var edgeRootToChildAsChildren = graph.Edges.Single(e => e.From == root && e.To == childVertex && (e as Edge).Value.IsCollection == false);
            var edgeRootToRootAsFavorite = graph.Edges.Single(e => e.From == root && e.To == childVertex && (e as Edge).Value.IsCollection == false);

            var tree = new Tree(root,
                new[] {
                    NoData.Graph.Base.ITuple.Create(edgeRootToChildAsChildren, new Tree(childVertex)),
                    NoData.Graph.Base.ITuple.Create(edgeRootToRootAsFavorite, new Tree(root)),
                }
            );
            var traversedEdges = new List<Edge>();
            tree.Traverse(e => traversedEdges.Add(e));
            Assert.AreEqual(2, traversedEdges.Count());
        }

        [Test]
        public void Tree_Flatten_Success()
        {
            var graph = Graph.CreateFromGeneric<Dto>();
            var root = graph.VertexContainingType(typeof(Dto));
            var childVertex = graph.VertexContainingType(typeof(DtoChild));

            var edgeRootToChildAsChildren = graph.Edges.Single(e => e.From == root && e.To == childVertex && e.Value.IsCollection == false);
            var edgeRootToRootAsFavorite = graph.Edges.Single(e => e.From == root && e.To == root && e.Value.IsCollection == false);

            var tree = new Tree(root,
                new[] {
                    NoData.Graph.Base.ITuple.Create(edgeRootToChildAsChildren, new Tree(childVertex)),
                    NoData.Graph.Base.ITuple.Create(edgeRootToRootAsFavorite, new Tree(root)),
                }
            );

            var flat = TreeUtility.Flatten(tree);
            Assert.AreEqual(2, flat.Vertices.Count());
            Assert.AreEqual(2, flat.Edges.Count());
            Assert.NotNull(flat.VertexOfValue(root.Value));
            Assert.NotNull(flat.VertexOfValue(childVertex.Value));
        }

        [Test]
        public void Tree_TraverseVertexEdges_Success()
        {
            var graph = Graph.CreateFromGeneric<Dto>();
            var root = graph.VertexContainingType(typeof(Dto));
            var childVertex = graph.VertexContainingType(typeof(DtoChild));
            var edgeRootToChildAsChildren = graph.Edges.Single(e => e.From == root && e.To == childVertex && e.Value.IsCollection == false);
            var edgeRootToRootAsFavorite = graph.Edges.Single(e => e.From == root && e.To == root && e.Value.IsCollection == false);
            var tree = new Tree(root,
                new[] {
                    NoData.Graph.Base.ITuple.Create(edgeRootToChildAsChildren, new Tree(childVertex)),
                    NoData.Graph.Base.ITuple.Create(edgeRootToRootAsFavorite, new Tree(root)),
                }
            );

            var list = new List<string>();
            tree.Traverse((Vertex v, IEnumerable<Edge> c) => list.Add($"{v.ToString()} - {string.Join(",", c.Select(e => e.ToString()))}"));

            Assert.AreEqual(3, list.Count());
            Assert.True(list[0].StartsWith("Dto - "));
            Assert.True(list[1] == "DtoChild - ");
            Assert.True(list[2] == "Dto - ");
        }
    }
}
