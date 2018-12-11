using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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
        private ParameterExpression DtoExpression = Expression.Parameter(typeof(Dto));
        private FilterClauseParser<Dto> Filter;
        IClassCache Cache = new ClassCache();
        GraphImplementations.Schema.GraphSchema _GraphSchema;

        private void SetFilter(string filterString)
        {
            Filter = new FilterClauseParser<Dto>(x => _GetTokens(x), filterString, TermHelper.FilterTerms);
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
        [InlineData("indexof(Name, 'eorge') eq 1")]
        [InlineData("contains(Name,'eorge')")]
        [InlineData("endswith(Name,'eorge')")]
        [InlineData("startswith(Name,'ge')")]
        [InlineData("replace(Name, 'Name', 'ReplacedName') eq 'ReplacedName'")]
        [InlineData("tolower('GeOrGe') eq 'george'")]
        [InlineData("toupper('GeOrGe') eq 'GEORGE'")]
        [InlineData("concat('Georg', 'e') eq 'George'")]
        [InlineData("trim(' George ') eq Name")]
        [InlineData("substring('George', 1) eq 'G'")]
        [InlineData("substring('George', 0, 1) eq 'G'")]
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
        public void FilterParser_TwoValueComparison_WithLogicComparison()
        {
            //Given
            SetFilter("id le 1 and id ge 1");

            //When
            Filter.Parse();
            var tree = Filter.Result;

            //Then
            Assert.True(Filter.IsFinished);
            tree.Should().NotBeNull();
            tree.Root.Value.Text.Should().Be(TextRepresentation.LogicalComparison); // root is logical compare
            tree.Root.Value.Representation.Should().Be(TextRepresentation.BooleanValue);
            tree.Children.First().Item2.Root.Value.Text.Should().Be(TextRepresentation.ValueComparison); // first child is value compare
            tree.Children.ToList()[1].Item2.Root.Value.Text.Should().Be("and"); // root compare text is "and"
            tree.Children.Last().Item2.Root.Value.Text.Should().Be(TextRepresentation.ValueComparison); // right side is value compare

            // left side of tree
            var lTree = tree.Children.First().Item2;
            lTree.Root.Value.Text.Should().Be(TextRepresentation.ValueComparison); // root is value compare
            lTree.Root.Value.Representation.Should().Be(TextRepresentation.BooleanValue);
            lTree.Children.First().Item2.Root.Value.Text.Should().Be("id"); // left side should be id text
            lTree.Children.ToList()[1].Item2.Root.Value.Text.Should().Be("le"); // root compare text is "le"
            lTree.Children.Last().Item2.Root.Value.Text.Should().Be("1"); // right side is value of 1
        }

        [Fact]
        public void FilterParser_Length()
        {
            //Given
            SetFilter("length(Name)");

            //When
            Filter.Parse();
            var tree = Filter.Result;

            //Then
            Assert.True(Filter.IsFinished);
            tree.Should().NotBeNull();
            tree.Root.Value.Representation.Should().Be(TextRepresentation.NumberValue);
            tree.Root.Value.Text.Should().Be(TextRepresentation.StrLength);
            tree.Children.Should().HaveCount(1);
            tree.Children.First().Item2.Root.Value.Representation.Should().Be(TextRepresentation.ExpandProperty);
            tree.Children.First().Item2.Root.Value.Text.Should().Be("Name");
        }

        [Fact]
        public void FilterParser_EndsWith()
        {
            //Given
            SetFilter("endswith(Name, Name)");

            //When
            Filter.Parse();
            var tree = Filter.Result;

            //Then
            Assert.True(Filter.IsFinished);
            tree.Should().NotBeNull();
            tree.Root.Value.Representation.Should().Be(TextRepresentation.BooleanValue);
            tree.Root.Value.Text.Should().Be(TextRepresentation.StrEndsWith);
            tree.Children.Should().HaveCount(2);
            tree.Children.First().Item2.Root.Value.Representation.Should().Be(TextRepresentation.ExpandProperty);
            tree.Children.First().Item2.Root.Value.Text.Should().Be("Name");
            tree.Children.Last().Item2.Root.Value.Representation.Should().Be(TextRepresentation.ExpandProperty);
            tree.Children.Last().Item2.Root.Value.Text.Should().Be("Name");
        }

        [Fact]
        public void FilterParser_StartsWith()
        {
            //Given
            SetFilter("startswith(Name, Name)");

            //When
            Filter.Parse();
            var tree = Filter.Result;

            //Then
            Assert.True(Filter.IsFinished);
            tree.Should().NotBeNull();
            tree.Root.Value.Representation.Should().Be(TextRepresentation.BooleanValue);
            tree.Root.Value.Text.Should().Be(TextRepresentation.StrStartsWith);
            tree.Children.Should().HaveCount(2);
            tree.Children.First().Item2.Root.Value.Representation.Should().Be(TextRepresentation.ExpandProperty);
            tree.Children.First().Item2.Root.Value.Text.Should().Be("Name");
            tree.Children.Last().Item2.Root.Value.Representation.Should().Be(TextRepresentation.ExpandProperty);
            tree.Children.Last().Item2.Root.Value.Text.Should().Be("Name");
        }

        [Fact]
        public void FilterParser_IndexOf()
        {
            //Given
            SetFilter("indexof(Name, Name)");

            //When
            Filter.Parse();
            var tree = Filter.Result;

            //Then
            Assert.True(Filter.IsFinished);
            tree.Should().NotBeNull();
            tree.Root.Value.Representation.Should().Be(TextRepresentation.NumberValue);
            tree.Root.Value.Text.Should().Be(TextRepresentation.StrIndexOf);
            tree.Children.Should().HaveCount(2);
            tree.Children.First().Item2.Root.Value.Representation.Should().Be(TextRepresentation.ExpandProperty);
            tree.Children.First().Item2.Root.Value.Text.Should().Be("Name");
            tree.Children.Last().Item2.Root.Value.Representation.Should().Be(TextRepresentation.ExpandProperty);
            tree.Children.Last().Item2.Root.Value.Text.Should().Be("Name");
        }

        [Fact]
        public void FilterParser_Contains()
        {
            //Given
            SetFilter("contains(Name, Name)");

            //When
            Filter.Parse();
            var tree = Filter.Result;

            //Then
            Assert.True(Filter.IsFinished);
            tree.Should().NotBeNull();
            tree.Root.Value.Representation.Should().Be(TextRepresentation.BooleanValue);
            tree.Root.Value.Text.Should().Be(TextRepresentation.StrContains);
            tree.Children.Should().HaveCount(2);
            tree.Children.First().Item2.Root.Value.Representation.Should().Be(TextRepresentation.ExpandProperty);
            tree.Children.First().Item2.Root.Value.Text.Should().Be("Name");
            tree.Children.Last().Item2.Root.Value.Representation.Should().Be(TextRepresentation.ExpandProperty);
            tree.Children.Last().Item2.Root.Value.Text.Should().Be("Name");
        }

        [Fact]
        public void FilterParser_Replace()
        {
            //Given
            SetFilter("replace(Name, Name, 'other')");

            //When
            Filter.Parse();
            var tree = Filter.Result;

            //Then
            Assert.True(Filter.IsFinished);
            tree.Should().NotBeNull();
            tree.Root.Value.Representation.Should().Be(TextRepresentation.TextValue);
            tree.Root.Value.Text.Should().Be(TextRepresentation.StrReplace);
            tree.Children.Should().HaveCount(3);
            tree.Children.First().Item2.Root.Value.Representation.Should().Be(TextRepresentation.ExpandProperty);
            tree.Children.First().Item2.Root.Value.Text.Should().Be("Name");
            tree.Children.ToList()[1].Item2.Root.Value.Representation.Should().Be(TextRepresentation.ExpandProperty);
            tree.Children.ToList()[1].Item2.Root.Value.Text.Should().Be("Name");
            tree.Children.Last().Item2.Root.Value.Representation.Should().Be(TextRepresentation.TextValue);
            tree.Children.Last().Item2.Root.Value.Text.Should().Be("'other'");
        }
    }
}