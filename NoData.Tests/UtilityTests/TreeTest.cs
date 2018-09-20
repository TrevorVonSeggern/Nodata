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
        [Fact]
        public void Graph_Tree_Flatten_SelectOneEdge()
        {
            var graph = NoData.GraphImplementations.Schema.GraphSchema.CreateFromGeneric<Dto>();

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
            var graph = NoData.GraphImplementations.Schema.GraphSchema.CreateFromGeneric<Dto>();

            var root = graph.VertexContainingType(typeof(Dto));
            var edge = graph.OutgoingEdges(root).First(x => x.From != x.To);
            var vertexCloned = edge.To.Clone() as Vertex;
            var edge2 = (Edge)edge.CloneWithNewReferences(root, vertexCloned);
            var tree = new Tree(root, new[] {
                Graph.ITuple.Create<Edge, Tree>(edge, new Tree(edge.To)),
                Graph.ITuple.Create<Edge, Tree>(edge2, new Tree(edge2.To)),
            });
            var flat = tree.Flatten();
            Assert.NotNull(flat);
            Assert.Equal(2, flat.Vertices.Count());
            Assert.Single(flat.Edges);
        }

        [Fact]
        public void Graph_Tree_SelectAsPathEnumerable()
        {
            var graph = NoData.GraphImplementations.Schema.GraphSchema.CreateFromGeneric<Dto>();

            var root = graph.VertexContainingType(typeof(Dto));
            var edge = graph.OutgoingEdges(root).First(x => x.From != x.To);
            var vertexCloned = edge.To.Clone() as Vertex;
            var edge2 = (Edge)edge.CloneWithNewReferences(root, vertexCloned);
            var tree = new Tree(root, new[] {
                Graph.ITuple.Create<Edge, Tree>(edge, new Tree(edge.To)),
                Graph.ITuple.Create<Edge, Tree>(edge2, new Tree(edge2.To)),
            });
            var pathList = tree.EnumerateAllPaths().ToList();
            Assert.NotNull(pathList);
            Assert.Equal(2, pathList.Count);
            Assert.Single(pathList.First());
        }
    }
}
