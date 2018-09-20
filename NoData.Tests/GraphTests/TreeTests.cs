using Xunit;
using System;
using System.Collections.Generic;
using System.Linq;
using NoData.Tests.SharedExampleClasses;
using NoData.GraphImplementations.Schema;
using NoData.Utility;

namespace NoData.Tests.GraphTests
{
    public class TreeTests
    {
        [Fact]
        public void Tree_Traverse_Success()
        {
            var graph = GraphSchema.CreateFromGeneric<Dto>();
            var root = graph.VertexContainingType(typeof(Dto));
            var childVertex = graph.VertexContainingType(typeof(DtoChild));

            var edgeRootToChildAsChildren = graph.Edges.Single(e => e.From == root && e.To == childVertex && (e as Edge).Value.IsCollection == false);
            var edgeRootToRootAsFavorite = graph.Edges.Single(e => e.From == root && e.To == childVertex && (e as Edge).Value.IsCollection == false);

            var tree = new Tree(root,
                new[] {
                    Graph.ITuple.Create(edgeRootToChildAsChildren, new Tree(childVertex)),
                    Graph.ITuple.Create(edgeRootToRootAsFavorite, new Tree(root)),
                }
            );
            var traversedEdges = new List<Edge>();
            tree.TraverseDepthFirstSearch(e => traversedEdges.Add(e));
            Assert.Equal(2, traversedEdges.Count);
        }

        [Fact]
        public void Tree_Flatten_Success()
        {
            var graph = GraphSchema.CreateFromGeneric<Dto>();
            var root = graph.VertexContainingType(typeof(Dto));
            var childVertex = graph.VertexContainingType(typeof(DtoChild));

            var edgeRootToChildAsChildren = graph.Edges.Single(e => e.From == root && e.To == childVertex && e.Value.IsCollection == false);
            var edgeRootToRootAsFavorite = graph.Edges.Single(e => e.From == root && e.To == root && e.Value.IsCollection == false);

            var tree = new Tree(root,
                new[] {
                    Graph.ITuple.Create(edgeRootToChildAsChildren, new Tree(childVertex)),
                    Graph.ITuple.Create(edgeRootToRootAsFavorite, new Tree(root)),
                }
            );

            var flat = tree.Flatten();
            Assert.Equal(2, flat.Vertices.Count());
            Assert.Equal(2, flat.Edges.Count());
            Assert.NotNull(flat.VertexOfValue(root.Value));
            Assert.NotNull(flat.VertexOfValue(childVertex.Value));
        }

        [Fact]
        public void Tree_TraverseVertexEdges_Success()
        {
            var graph = GraphSchema.CreateFromGeneric<Dto>();
            var root = graph.VertexContainingType(typeof(Dto));
            var childVertex = graph.VertexContainingType(typeof(DtoChild));
            var edgeRootToChildAsChildren = graph.Edges.Single(e => e.From == root && e.To == childVertex && e.Value.IsCollection == false);
            var edgeRootToRootAsFavorite = graph.Edges.Single(e => e.From == root && e.To == root && e.Value.IsCollection == false);
            var tree = new Tree(root,
                new[] {
                    Graph.ITuple.Create(edgeRootToChildAsChildren, new Tree(childVertex)),
                    Graph.ITuple.Create(edgeRootToRootAsFavorite, new Tree(root)),
                }
            );

            var list = new List<string>();
            tree.TraverseDepthFirstSearch((Vertex v, IEnumerable<Edge> c) => list.Add($"{v.ToString()} - {string.Join(",", c.Select(e => e.ToString()))}"));

            Assert.Equal(3, list.Count);
            Assert.StartsWith("Dto - ", list[0]);
            Assert.Equal("DtoChild - ", list[1]);
            Assert.Equal("Dto - ", list[2]);
        }
    }
}
