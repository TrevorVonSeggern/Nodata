using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NoData.GraphImplementations.QueryParser;
using NoData.Internal.TreeParser.Tokenizer;
using NoData.QueryParser;
using NoData.QueryParser.ParsingTools;
using NoData.Tests.SharedExampleClasses;
using NoData.Utility;
using Xunit;
using QueueItem = NoData.GraphImplementations.QueryParser.Tree;

namespace NoData.Tests.ParserTests.Parsers
{
    public class FilterParserTests
    {
        private FilterClauseParser<Dto> Filter;
        IClassCache Cache = new ClassCache();
        GraphImplementations.Schema.GraphSchema _GraphSchema;

        private void SetFilter(string filterString)
        {
            Filter = new FilterClauseParser<Dto>(x => _GetTokens(x), filterString, _TermHelper.FilterTerms);
        }

        private IList<QueueItem> _GetTokens(string parameter)
        {
            var tokens = new Tokenizer(_GraphSchema.VertexContainingType(typeof(Dto)).Value.Properties.Select(x => x.Name)).Tokenize(parameter);
            return tokens.Select(t => new QueueItem(new Vertex(t))).ToList();
        }

        public FilterParserTests()
        {
            _GraphSchema = GraphImplementations.Schema.GraphSchema.Cache<Dto>.Graph;
        }

        [Theory]
        [InlineData("length(Name) eq 1")]
        [InlineData("concat('Georg', 'e') eq 'George'")]
        [InlineData("substring('George', 1) eq 'G'")]
        [InlineData("substring('George', 0, 1) eq 'G'")]
        [InlineData("trim(' George ') eq Name")]
        [InlineData("tolower('GeOrGe ' eq 'george'")]
        [InlineData("toupper('GeOrGe ' eq 'GEORGE'")]
        [InlineData("contains(Name,'eorge')")]
        [InlineData("endswith(Name,'eorge')")]
        [InlineData("startswith(Name,'ge')")]
        [InlineData("replace(Name, 'Name', 'ReplacedName') eq 'ReplacedName'")]
        [InlineData("indexof(Name, 'eorge') eq 1")]
        public void FilterParser_ParseResult_NotNull(string input)
        {
            //Given
            SetFilter(input);

            //When
            Filter.Parse();
            var tree = Filter.Result;

            //Then
            Assert.True(Filter.IsFinished);
            Filter.Result.Should().NotBeNull();
        }

        [Fact]
        public void FilterParser_NameEqConst()
        {
            //Given
            SetFilter("Name eq 1");

            //When
            Filter.Parse();
            var tree = Filter.Result;

            //Then
            Assert.True(Filter.IsFinished);
            tree.Should().NotBeNull();
            tree.Root.Value.Text.Should().Be(TextRepresentation.ValueComparison); // root is eq
            tree.Root.Value.Representation.Should().Be(TextRepresentation.BooleanValue);
            tree.Children.First().Item2.Root.Value.Text.Should().Be("Name"); // first child is name
            tree.Children.ToList()[1].Item2.Root.Value.Text.Should().Be("eq"); // seccond child is 1
            tree.Children.Last().Item2.Root.Value.Text.Should().Be("1"); // seccond child is 1
        }

        [Fact]
        public void FilterParser_Length()
        {
            //Given
            SetFilter("length(Name) eq 1");

            //When
            Filter.Parse();
            var tree = Filter.Result;

            //Then
            Assert.True(Filter.IsFinished);
            Filter.Result.Should().NotBeNull();
        }
    }
}