using NUnit.Framework;
using NoData.Tests.SharedExampleClasses;
using NoData.QueryParser.ParsingTools;
using System.Collections.Generic;
using QueueItem = NoData.QueryParser.Graph.Tree;
using NoData.Internal.TreeParser.Tokenizer;
using System.Linq;
using NoData.QueryParser.ParsingTools.Groupings;

namespace NoData.Tests.ExpandTests
{
    [TestFixture]
    public class ExpandGrouper_FakeExpandProperty
    {

        private IList<QueueItem> _GetTokens(string parmeter) => new Tokenizer(ClassProperties).Tokenize(parmeter).Select(t => new QueueItem(new NoData.QueryParser.Graph.Vertex(t), t.Value)).ToList();
        private readonly IEnumerable<string> ClassProperties = NoData.Utility.ClassInfoCache.GetOrAdd(typeof(Dto)).PropertyNames;

        [Test]
        public void ExpandGrouper_CanGroupExpandPropertyCorrectly()
        {
            // setup
            var queryString = nameof(Dto.partner);
            var tokens = _GetTokens(queryString);

            var grouper = new QueueGrouper<QueueItem>(tokens, QueueItem.GetRepresentationValue);

            grouper.AddGroupingTerm(ExpandGroupings.ExpandProperty);

            var parsed = grouper.Reduce();

            Assert.NotNull(parsed);
            Assert.True(parsed.IsPropertyAccess);
            Assert.False(parsed.IsFakeExpandProperty);
        }

        [Test]
        public void ExpandGrouper_CanGroupFakeExpandPropertyCorrectly()
        {
            // setup
            var queryString = $"{nameof(Dto.partner)}($expand={nameof(Dto.partner)})";
            var tokens = _GetTokens(queryString);

            var grouper = new QueueGrouper<QueueItem>(tokens, QueueItem.GetRepresentationValue);

            grouper.AddGroupingTerm(ExpandGroupings.ExpandProperty);
            grouper.AddGroupingTerm(ExpandGroupings.ClauseExpressionGrouper());
            grouper.AddGroupingTerm(ExpandGroupings.ExpandPropertyWithEmptyParenthesis);
            grouper.AddGroupingTerm(FilterGroupings.AddTermsForFilter());
            grouper.AddGroupingTerm(ExpandGroupings.ClauseIntegrations);
            grouper.AddGroupingTerm(ExpandGroupings.CollectionOfExpandProperty);

            var parsed = grouper.Reduce();

            Assert.NotNull(parsed);
            Assert.NotNull(parsed.Root);
            Assert.NotNull(parsed.Children);
            Assert.NotZero(parsed.Children.Count());
            foreach (var child in parsed.Children.Select(x => x.Item2))
            {
                Assert.True(child.IsPropertyAccess);
                Assert.True(child.IsFakeExpandProperty);
                Assert.False(child.IsDirectPropertyAccess);
            }
        }
    }
}