using System;
using System.Collections.Generic;
using System.Linq;
using NoData.GraphImplementations.Schema;
using NoData.Utility;
using NoData.Tests.SharedExampleClasses;
using Xunit;

namespace NoData.Tests.GraphTests
{

    public class GraphTests
    {
        [Fact]
        public void Graph_VertexContainsTypes_Success()
        {
            var graph = GraphSchema.CreateFromGeneric<Dto>();

            var types = graph.Vertices.Select(v => (v.Value as ClassInfo)?.Type).ToList();
            Assert.Contains(typeof(Dto), types); // must contain dto vertex
            Assert.Contains(typeof(DtoChild), types); // must contain child dto vertex
            Assert.Contains(typeof(DtoGrandChild), types); // must contain grand child dto vertex

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
            var graph = GraphSchema.CreateFromGeneric<Dto>();

            var edge = graph.Edges.SingleOrDefault(e =>
                (e.From.Value as ClassInfo).Type == from &&
                (e.To.Value as ClassInfo).Type == to &&
                e.Value.PropertyName == propertyName &&
                e.Value.IsCollection == isCollection
            );

            Assert.NotNull(edge); // $"Edge must exist: from: {from}, to: {to}, with name: {propertyName}, collection: {isCollection}");
        }
    }
}
