using System.Collections.Generic;
using System.Linq.Expressions;
using FluentAssertions;
using NoData.Tests.SharedExampleClasses;
using NoData.Utility;
using Xunit;

namespace NoData.Tests.ParserTests
{
    public class FilterQueryParserTests
    {
        ParameterExpression DtoExpression = Expression.Parameter(typeof(Dto), "Dto");
        QueryParser.QueryParser<Dto> Parser;
        GraphImplementations.Schema.GraphSchema _GraphSchema;
        IClassCache Cache = new ClassCache();

        private void SetParser(string filterInput)
        {
            var parameters = new Parameters(null, filterInput);
            Parser = new QueryParser.QueryParser<Dto>(parameters, _GraphSchema, Cache);
        }

        public FilterQueryParserTests()
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
        public void FilterParserTests_StrFunctions_CanParse_StringFunctions(string toParse)
        {
            //Given
            SetParser(toParse);

            //When
            Assert.True(Parser.IsParsed);

            //Then
            Parser.ApplyFilterExpression(DtoExpression).Should().NotBeNull();
        }

        [Theory]
        [InlineData("id le 1 and id ge 1")]
        public void FilterParserTests_Simple_CanParse(string toParse)
        {
            //Given
            SetParser(toParse);

            //When
            Assert.True(Parser.IsParsed);

            //Then
            Parser.ApplyFilterExpression(DtoExpression).Should().NotBeNull();
        }

        [Fact]
        public void FilterParserTests_StrFunctions_CanParse_Length()
        {
            //Given
            SetParser("length(Name)");

            //When
            Assert.True(Parser.IsParsed);

            //Then
            Parser.ApplyFilterExpression(DtoExpression).Should().NotBeNull();
        }

        [Fact]
        public void FilterParserTests_StrFunctions_CanParse_EndsWith()
        {
            //Given
            SetParser("endswith(Name, Name)");

            //When
            Assert.True(Parser.IsParsed);

            //Then
            Parser.ApplyFilterExpression(DtoExpression).Should().NotBeNull();
        }

        [Fact]
        public void FilterParserTests_StrFunctions_CanParse_StartsWith()
        {
            //Given
            SetParser("startswith(Name, Name)");

            //When
            Assert.True(Parser.IsParsed);

            //Then
            Parser.ApplyFilterExpression(DtoExpression).Should().NotBeNull();
        }

        [Fact]
        public void FilterParserTests_StrFunctions_CanParse_IndexOf()
        {
            //Given
            SetParser("indexof(Name, Name)");

            //When
            Assert.True(Parser.IsParsed);

            //Then
            Parser.ApplyFilterExpression(DtoExpression).Should().NotBeNull();
        }

        [Fact]
        public void FilterParserTests_StrFunctions_CanParse_Contains()
        {
            //Given
            SetParser("contains(Name, Name)");

            //When
            Assert.True(Parser.IsParsed);

            //Then
            Parser.ApplyFilterExpression(DtoExpression).Should().NotBeNull();
        }

        [Fact]
        public void FilterParserTests_StrFunctions_CanParse_Replace()
        {
            //Given
            SetParser("replace(Name, Name, 'george') eq 'george'");

            //When
            Assert.True(Parser.IsParsed);

            //Then
            Parser.ApplyFilterExpression(DtoExpression).Should().NotBeNull();
        }
    }
}