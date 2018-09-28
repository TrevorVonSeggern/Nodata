using System.Linq;
using FluentAssertions;
using NoData.Tests.SharedExampleClasses;
using Xunit;

namespace NoData.Tests.IntegrationTests
{
    public class OrderByTests
    {
        [Fact]
        public void OrderBy_Id_InOrder()
        {
            var dtoEnum = Enumerable.Range(0, 100).Select(x => new Dto { id = 100 - x, Name = (-1).ToString(), region_code = "en" });
            var ids = new NoData.NoDataBuilder<Dto>(null, null, null, nameof(Dto.id)).Load(dtoEnum.AsQueryable()).BuildQueryable().Select(x => x.id).ToList();
            ids.Should().BeInAscendingOrder();
        }

        [Fact]
        public void OrderBy_Name_InOrder()
        {
            var dtoEnum = Enumerable.Range(0, 100).Select(x => new Dto { id = -1, Name = (100 - x).ToString(), region_code = "en" });
            var names = new NoData.NoDataBuilder<Dto>(null, null, null, nameof(Dto.Name)).Load(dtoEnum.AsQueryable()).BuildQueryable().Select(x => x.Name).ToList();
            names.Should().BeInAscendingOrder();
        }

        [Fact]
        public void OrderBy_Id_InOrder_Asc()
        {
            var dtoEnum = Enumerable.Range(0, 100).Select(x => new Dto { id = 100 - x, Name = (-1).ToString(), region_code = "en" });
            var ids = new NoData.NoDataBuilder<Dto>(null, null, null, $"{nameof(Dto.id)} asc").Load(dtoEnum.AsQueryable()).BuildQueryable().Select(x => x.id).ToList();
            ids.Should().BeInAscendingOrder();
        }

        [Fact]
        public void OrderBy_Name_InOrder_Asc()
        {
            var dtoEnum = Enumerable.Range(0, 100).Select(x => new Dto { id = -1, Name = (100 - x).ToString(), region_code = "en" });
            var names = new NoData.NoDataBuilder<Dto>(null, null, null, $"{nameof(Dto.Name)} asc").Load(dtoEnum.AsQueryable()).BuildQueryable().Select(x => x.Name).ToList();
            names.Should().BeInAscendingOrder();
        }

        [Fact]
        public void OrderBy_Id_InOrder_Desc()
        {
            var dtoEnum = Enumerable.Range(0, 100).Select(x => new Dto { id = 100 - x, Name = (-1).ToString(), region_code = "en" });
            var ids = new NoData.NoDataBuilder<Dto>(null, null, null, $"{nameof(Dto.id)} desc").Load(dtoEnum.AsQueryable()).BuildQueryable().Select(x => x.id).ToList();
            ids.Should().BeInDescendingOrder();
        }

        [Fact]
        public void OrderBy_Name_InOrder_Desc()
        {
            var dtoEnum = Enumerable.Range(0, 100).Select(x => new Dto { id = -1, Name = (100 - x).ToString(), region_code = "en" });
            var names = new NoData.NoDataBuilder<Dto>(null, null, null, $"{nameof(Dto.Name)} desc").Load(dtoEnum.AsQueryable()).BuildQueryable().Select(x => x.Name).ToList();
            names.Should().BeInDescendingOrder();
        }

        [Fact]
        public void OrderBy_NameThenId_InOrder_Asc()
        {
            string query = $"{nameof(Dto.Name)} asc,{nameof(Dto.id)} asc";
            var dtoEnum = Enumerable.Range(0, 100).Select(x => new Dto { id = x, Name = (x % 3).ToString(), region_code = "en" });
            var dtos = new NoData.NoDataBuilder<Dto>(null, null, null, query).Load(dtoEnum.AsQueryable()).BuildQueryable().ToList();
            Assert.Equal(dtos, dtos.OrderBy(x => x.Name).ThenBy(x => x.id));
        }

        [Fact]
        public void OrderBy_IdThenName_InOrder_Asc()
        {
            string query = $"{nameof(Dto.id)} asc,{nameof(Dto.Name)} asc";
            var dtoEnum = Enumerable.Range(0, 100).Select(x => new Dto { id = x, Name = (x % 3).ToString(), region_code = "en" });
            var dtos = new NoData.NoDataBuilder<Dto>(null, null, null, query).Load(dtoEnum.AsQueryable()).BuildQueryable().ToList();

            Assert.Equal(dtos, dtos.OrderBy(x => x.id).ThenBy(x => x.Name));
        }

        [Fact]
        public void OrderBy_IdThenName_InOrder_Desc()
        {
            string query = $"{nameof(Dto.id)} desc,{nameof(Dto.Name)} desc";
            var dtoEnum = Enumerable.Range(0, 100).Select(x => new Dto { id = x, Name = (x % 3).ToString(), region_code = "en" });
            var dtos = new NoData.NoDataBuilder<Dto>(null, null, null, query).Load(dtoEnum.AsQueryable()).BuildQueryable().ToList();

            Assert.Equal(dtos, dtos.OrderByDescending(x => x.id).ThenByDescending(x => x.Name));
        }

        [Fact]
        public void OrderBy_IdThenId_InOrder()
        {
            string query = $"{nameof(Dto.id)} asc,{nameof(Dto.id)} asc";
            var dtoEnum = Enumerable.Range(0, 100).Select(x => new Dto { id = x, Name = (x % 3).ToString(), region_code = "en" });
            var dtos = new NoData.NoDataBuilder<Dto>(null, null, null, query).Load(dtoEnum.AsQueryable()).BuildQueryable().ToList();

            Assert.Equal(dtos, dtos.OrderBy(x => x.id));
        }

        [Fact]
        public void OrderBy_IdThenId_InOrder_Desc()
        {
            string query = $"{nameof(Dto.id)} desc,{nameof(Dto.id)} desc";
            var dtoEnum = Enumerable.Range(0, 100).Select(x => new Dto { id = x, Name = (x % 3).ToString(), region_code = "en" });
            var dtos = new NoData.NoDataBuilder<Dto>(null, null, null, query).Load(dtoEnum.AsQueryable()).BuildQueryable().ToList();

            Assert.Equal(dtos, dtos.OrderByDescending(x => x.id));
        }

        [Fact]
        public void OrderBy_IdAscThenIdDesc_InOrder_Asc()
        {
            string query = $"{nameof(Dto.id)} asc,{nameof(Dto.id)} desc";
            var dtoEnum = Enumerable.Range(0, 100).Select(x => new Dto { id = x, Name = (x % 3).ToString(), region_code = "en" });
            var dtos = new NoData.NoDataBuilder<Dto>(null, null, null, query).Load(dtoEnum.AsQueryable()).BuildQueryable().ToList();

            Assert.Equal(dtos, dtos.OrderBy(x => x.id));
        }

        [Fact]
        public void OrderBy_IdThenName_InOrder_Default_Asc()
        {
            string query = $"{nameof(Dto.id)},{nameof(Dto.Name)}";
            var dtoEnum = Enumerable.Range(0, 100).Select(x => new Dto { id = x, Name = (x % 3).ToString(), region_code = "en" });
            var dtos = new NoData.NoDataBuilder<Dto>(null, null, null, query).Load(dtoEnum.AsQueryable()).BuildQueryable().ToList();

            Assert.Equal(dtos, dtos.OrderBy(x => x.id).ThenBy(x => x.Name));
        }

        [Fact]
        public void OrderBy_DtoChildId_InOrder_Default()
        {
            string query = $"{nameof(Dto.favorite)}/{nameof(DtoChild.id)}";
            var dtoEnum = Enumerable.Range(0, 100).Select(x => new Dto { id = x, Name = (x % 3).ToString(), region_code = "en", favorite = new DtoChild { id = x % 5 } });
            var dtos = new NoData.NoDataBuilder<Dto>(nameof(Dto.favorite), null, null, query).Load(dtoEnum.AsQueryable()).BuildQueryable().ToList();

            Assert.Equal(dtos, dtos.OrderBy(x => x.favorite.id));
        }
    }
}