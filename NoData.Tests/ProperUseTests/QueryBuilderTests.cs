using System.Collections.Generic;
using Moq;
using NoData.Tests.SharedExampleClasses;
using NoData.Tests.SharedExampleClasses.Database.Entity;
using NoData.Tests.SharedExampleClasses.Database.Models;
using Xunit;
using Bogus;
using System.Linq;

namespace NoData.Tests.ProperUseTests
{
    public class QueryBuilderTests
    {
        Mock<NoDataBuilder<Dto>> _NoDataBuilderMock;
        NoDataBuilder<Dto> _NoDataBuilder => _NoDataBuilderMock.Object;

        Mock<INoDataQuery<Dto>> _NoDataQueryMock;
        INoDataQuery<Dto> _NoDataQuery => _NoDataQueryMock.Object;

        private static IEnumerable<Dto> GenerateData()
        {
            var r = new Faker<Dto>()
                .RuleFor(x => x.Name, (f, d) => f.Person.FirstName)
                .RuleFor(x => x.children, (f, d) => new List<SharedExampleClasses.DtoChild>());

            for (int i = 0; i < 10; ++i)
                yield return r.Generate();
        }

        public QueryBuilderTests()
        {
            _NoDataQueryMock = new Mock<INoDataQuery<Dto>>();
            _NoDataBuilderMock = new Mock<NoDataBuilder<Dto>>(new Parameters());
            _NoDataBuilderMock.Setup(x => x.Load(It.IsAny<IQueryable<Dto>>())).Returns(_NoDataQuery);
        }

        [Fact]
        public void QueryBuilder_Constructor()
        {
            var builder = _NoDataBuilder;
            var queryable = GenerateData().ToList().AsQueryable();
            builder.Load(queryable);
            _NoDataBuilderMock.Verify(m => m.Load(It.IsAny<IQueryable<Dto>>()), Times.Once());
            _NoDataQueryMock.Verify(m => m.BuildQueryable(), Times.Never());
            _NoDataQueryMock.Verify(m => m.AsJson(), Times.Never());
        }
    }
}