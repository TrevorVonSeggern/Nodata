using System.Collections.Generic;
using System.Linq;
using Xunit;
using NoData.Utility;
using NoData.GraphImplementations.Schema;
using NoData.Tests.SharedExampleClasses;

namespace NoData.Tests.UtilityTests
{
    public class TreeFlattenTest
    {
        GraphSchema graph;

        public TreeFlattenTest()
        {
            graph = GraphSchema.Cache<Dto>.Graph;
        }

        [Fact]
        public void Graph_Tree_Flatten_SelectOneEdge()
        {
            var root = graph.VertexContainingType(typeof(Dto));
            var edge = graph.OutgoingEdges(root).First(x => x.From != x.To);
            var tree = new Tree(root, new[] {
                Graph.ITuple.Create<Edge, Tree>(edge, new Tree(edge.To)),
            });
            var flat = tree.Flatten();
            Assert.NotNull(flat);
            Assert.Equal(2, flat.Vertices.Count());
            Assert.Single(flat.Edges);
        }

        [Fact]
        public void Graph_Tree_Flatten_SelectTwoEdges()
        {
            var root = graph.VertexContainingType(typeof(Dto));
            var edge = graph.OutgoingEdges(root).First(x => x.From != x.To);
            var tree = new Tree(root, new[] {
                Graph.ITuple.Create<Edge, Tree>(edge, new Tree(edge.To)),
                Graph.ITuple.Create<Edge, Tree>(edge, new Tree(edge.To)),
            });
            var flat = tree.Flatten();
            Assert.NotNull(flat);
            Assert.Equal(2, flat.Vertices.Count());
            Assert.Single(flat.Edges);
        }

        [Fact]
        public void Graph_Tree_SelectAsPathEnumerable()
        {
            var root = graph.VertexContainingType(typeof(Dto));
            var edge = graph.OutgoingEdges(root).First(x => x.From != x.To);
            var tree = new Tree(root, new[] {
                Graph.ITuple.Create<Edge, Tree>(edge, new Tree(edge.To)),
                Graph.ITuple.Create<Edge, Tree>(edge, new Tree(edge.To)),
            });
            var pathList = tree.EnumerateAllPaths().ToList();
            Assert.NotNull(pathList);
            Assert.Equal(2, pathList.Count);
            Assert.Single(pathList.First());
        }
    }
}
