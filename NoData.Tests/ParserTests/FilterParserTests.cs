using System.Linq.Expressions;
using NoData.Tests.SharedExampleClasses;
using NoData.Utility;
using Xunit;

namespace NoData.Tests.ParserTests
{
    public class FilterParserTests
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

        public FilterParserTests()
        {
            _GraphSchema = GraphImplementations.Schema.GraphSchema.Cache<Dto>.Graph;
        }

        [Fact]
        public void FilterParserTests_StrFunctions_CanParse_Length()
        {
            //Given
            SetParser("length(Name) eq 1");

            //When
            Assert.True(Parser.IsParsed);

            //Then
            Parser.ApplyFilterExpression(DtoExpression);
        }
    }
}