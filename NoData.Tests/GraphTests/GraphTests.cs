using NoData.Graph;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BinaryExpressionParserTests
{
    [TestFixture]
    public class GraphTests
    {
        public class Dto
        {
            public int id { get; set; }
            public string Name { get; set; }
            public string region_code { get; set; }
            public Dto partner { get; set; }
            public ICollection<DtoChild> children { get; set; }
            public DtoChild favorite { get; set; }
        }

        public class DtoChild
        {
            public int id { get; set; }
            public string Name { get; set; }
            public string region_code { get; set; }
            public DtoChild partner { get; set; }
            public ICollection<DtoGrandChild> children { get; set; }
            public DtoGrandChild favorite { get; set; }
        }

        public class DtoGrandChild
        {
            public int id { get; set; }
            public string Name { get; set; }
            public string region_code { get; set; }
        }

        [Test]
        public void Graph_VertexContainsTypes_Success()
        {
            var graph = Graph.CreateFromGeneric<Dto>();

            var types = graph.Vertices.Select(v => (v.Value as ClassInfo)?.Type).ToList();
            Assert.Contains(typeof(Dto), types, "must contain dto vertex");
            Assert.Contains(typeof(DtoChild), types, "must contain child dto vertex");
            Assert.Contains(typeof(DtoGrandChild), types, "must contain grand child dto vertex");

            Assert.AreEqual(3, graph.Vertices.Count());
        }

        [TestCase(typeof(Dto), typeof(DtoChild), nameof(Dto.favorite), false)]
        [TestCase(typeof(Dto), typeof(Dto), nameof(Dto.partner), false)]
        [TestCase(typeof(Dto), typeof(DtoChild), nameof(Dto.children), true)]
        [TestCase(typeof(DtoChild), typeof(DtoGrandChild), nameof(DtoChild.children), true)]
        [TestCase(typeof(DtoChild), typeof(DtoGrandChild), nameof(DtoChild.favorite), false)]
        [TestCase(typeof(DtoChild), typeof(DtoChild), nameof(DtoChild.partner), false)]
        public void Graph_Edges_SingleExists_Success(Type from, Type to, string propertyName, bool isCollection)
        {
            var graph = Graph.CreateFromGeneric<Dto>();

            Assert.DoesNotThrow(() =>
            {
                var edge = graph.Edges.SingleOrDefault(e =>
                    (e.From.Value as ClassInfo).Type == from &&
                    (e.To.Value as ClassInfo).Type == to &&
                    e.Value.PropertyName == propertyName &&
                    e.Value.IsCollection == isCollection
                );

                Assert.NotNull(edge , $"Edge must exist: from: {from}, to: {to}, with name: {propertyName}, collection: {isCollection}");
            }, "Exactly one edge expected.");
        }
    }
}
