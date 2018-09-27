using NoData.Tests.SharedExampleClasses;
using NoData.QueryParser.ParsingTools;
using System.Collections.Generic;
using NoData.Internal.TreeParser.Tokenizer;
using System.Linq;
using NoData.QueryParser.ParsingTools.Groupings;
using Xunit;

using QueueItem = NoData.GraphImplementations.QueryParser.Tree;
using NoData.GraphImplementations.QueryParser;
using NoData.Utility;
using NoData.QueryParser;
using System;
using System.Text.RegularExpressions;
using Graph;

namespace NoData.Tests.ExpandTests
{
    public class ExpandGrouper_FakeExpandProperty : IClassFixture<NoData.Tests.SharedExampleClasses.CacheAndGraphFixture<Dto>>
    {
        CacheAndGraphFixture<Dto> Fixture;
        private IList<QueueItem> _GetTokens(string parmeter) => new Tokenizer(ClassProperties).Tokenize(parmeter).Select(t => new QueueItem(new Vertex(t))).ToList();
        private readonly IEnumerable<string> ClassProperties;

        public ExpandGrouper_FakeExpandProperty(CacheAndGraphFixture<Dto> fixture)
        {
            Fixture = fixture;
            ClassProperties = fixture.cache.GetOrAdd(typeof(Dto)).PropertyNames;
        }


        [Fact]
        public void ExpandGrouper_CanGroupExpandPropertyCorrectly()
        {
            // setup
            var queryString = nameof(Dto.partner);
            var tokens = _GetTokens(queryString);
            var dict = new Dictionary<Regex, Func<IList<QueueItem>, ITuple<QueueItem, int>>>();
            var term = ExpandGroupings.ExpandProperty;
            dict.Add(new Regex(term.Item1, RegexOptions.Compiled | RegexOptions.Singleline), term.Item2);
            var grouper = new QueueGrouper<QueueItem>(dict);
            var parsed = grouper.ParseToSingle(tokens);

            Assert.NotNull(parsed);
            Assert.True(parsed.IsPropertyAccess());
            Assert.False(parsed.IsFakeExpandProperty());
        }

        [Fact]
        public void ExpandGrouper_CanGroupFakeExpandPropertyCorrectly()
        {
            // setup
            var queryString = $"{nameof(Dto.partner)}($expand={nameof(Dto.partner)}($expand={nameof(Dto.partner)}))";
            var tokens = _GetTokens(queryString);

            var grouper = new OrderdGrouper<QueueItem>(_TermHelper.ExpandTerms);

            var parsed = grouper.ParseToSingle(tokens);

            Assert.NotNull(parsed);
            Assert.NotNull(parsed.Root);
            Assert.NotNull(parsed.Children);
            Assert.NotEmpty(parsed.Children);
            foreach (var child in parsed.Children.Select(x => x.Item2))
            {
                Assert.True(child.IsPropertyAccess());
                Assert.False(child.IsFakeExpandProperty());
                Assert.False(child.IsDirectPropertyAccess());
            }
        }
    }
}