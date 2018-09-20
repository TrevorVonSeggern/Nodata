using NoData.Tests.SharedExampleClasses;
using NoData.QueryParser.ParsingTools;
using System.Collections.Generic;
using NoData.Internal.TreeParser.Tokenizer;
using System.Linq;
using NoData.QueryParser.ParsingTools.Groupings;
using Xunit;

using QueueItem = NoData.GraphImplementations.QueryParser.Tree;
using NoData.GraphImplementations.QueryParser;

namespace NoData.Tests.ExpandTests
{
    public class ExpandGrouper_FakeExpandProperty
    {

        private IList<QueueItem> _GetTokens(string parmeter) => new Tokenizer(ClassProperties).Tokenize(parmeter).Select(t => new QueueItem(new Vertex(t))).ToList();
        private readonly IEnumerable<string> ClassProperties = NoData.Utility.ClassInfoCache.GetOrAdd(typeof(Dto)).PropertyNames;

        [Fact]
        public void ExpandGrouper_CanGroupExpandPropertyCorrectly()
        {
            // setup
            var queryString = nameof(Dto.partner);
            var tokens = _GetTokens(queryString);

            var grouper = new QueueGrouper<QueueItem>();
            grouper.AddGroupingTerms(ExpandGroupings.ExpandProperty);
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

            var grouper = new OrderdGrouper<QueueItem>();

            grouper.AddGroupingTerms(ExpandGroupings.ExpandProperty);
            grouper.AddGroupingTerms(FilterGroupings.AddTermsForFilter());
            grouper.AddGroupingTerms(ExpandGroupings.ExpandExpression);
            grouper.AddGroupingTerms(ExpandGroupings.FilterExpression);
            grouper.AddGroupingTerms(ExpandGroupings.SelectExpression);
            grouper.AddGroupingTerms(ExpandGroupings.ListOfClauseExpressions());
            grouper.AddGroupingTerms(ExpandGroupings.ExpandPropertyWithListOfClauses);
            grouper.AddGroupingTerms(ExpandGroupings.ListOfExpand);

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