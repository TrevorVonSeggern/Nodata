using System;
using System.Collections.Generic;
using System.Linq;
using NoData.GraphImplementations.Schema;
using NoData.Utility;
using NoData.Tests.SharedExampleClasses;
using Xunit;

namespace NoData.Tests.GraphSchemaTests
{
    public class GraphTests
    {
        IClassCache cache;

        public GraphTests()
        {
            cache = new ClassCache();
        }

        [Fact]
        public void Graph_VertexContainsTypes_Success()
        {
            var graph = GraphSchema.Cache<Dto>.Graph;

            var types = graph.Vertices.Select(v => v.Value.TypeId).ToList();
            Assert.Contains(typeof(Dto).GetHashCode(), types); // must contain dto vertex
            Assert.Contains(typeof(DtoChild).GetHashCode(), types); // must contain child dto vertex
            Assert.Contains(typeof(DtoGrandChild).GetHashCode(), types); // must contain grand child dto vertex

            Assert.Equal(3, graph.Vertices.Count());
        }

        [Theory]
        [InlineData(typeof(Dto), typeof(DtoChild), nameof(Dto.favorite), false)]
        [InlineData(typeof(Dto), typeof(Dto), nameof(Dto.partner), false)]
        [InlineData(typeof(Dto), typeof(DtoChild), nameof(Dto.children), true)]
        [InlineData(typeof(DtoChild), typeof(DtoGrandChild), nameof(DtoChild.children), true)]
        [InlineData(typeof(DtoChild), typeof(DtoGrandChild), nameof(DtoChild.favorite), false)]
        [InlineData(typeof(DtoChild), typeof(DtoChild), nameof(DtoChild.partner), false)]
        public void Graph_Edges_SingleExists_Success(Type from, Type to, string propertyName, bool isCollection)
        {
            var graph = GraphSchema.Cache<Dto>.Graph;

            var edge = graph.Edges.SingleOrDefault(e =>
                (e.From.Value as ClassInfo).TypeId == from.GetHashCode() &&
                (e.To.Value as ClassInfo).TypeId == to.GetHashCode() &&
                e.Value.Name == propertyName &&
                e.Value.IsCollection == isCollection
            );

            Assert.NotNull(edge); // $"Edge must exist: from: {from}, to: {to}, with name: {propertyName}, collection: {isCollection}");
        }
    }
}
