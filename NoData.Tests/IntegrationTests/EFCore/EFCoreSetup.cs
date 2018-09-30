using System.Linq;
using AutoMapper;
using NoData.Tests.SharedExampleClasses.Database;
using NoData.Tests.SharedExampleClasses.Database.Entity;
using NoData.Tests.SharedExampleClasses.Database.Models;
using NoData.Tests.SharedExampleClasses.Automapper;
using Microsoft.EntityFrameworkCore;
using AutoMapper.QueryableExtensions;
using System.Collections.Generic;
using Xunit;

namespace NoData.Tests.IntegrationTests.EFCore
{
    public class EFCoreSetup
    {
        private DataContext GetDataContext => SharedExampleClasses.Database.DataContext.GetInMemoryDatabase();
        private IMapper GetMapper => SharedExampleClasses.Automapper.AutoMapperConfig.CreateConfig();

        [Fact]
        public void EfCoreSetup_CanInitializeDatabase()
        {
            var mapper = GetDataContext;
        }

        [Fact]
        public void EfCoreSetup_AutomapperConfigIsValid()
        {
            var mapper = GetMapper;
        }


        [Fact]
        public void EfCoreSetup_Database_CanInsertPerson_andGetBackPerson()
        {
            var context = GetDataContext;
            context.People.Add(new Person { Id = 1, Region_code = "en-US", Name = "Don't fail plz" });
            context.SaveChanges();
            Assert.Equal(1, context.People.Count());
            Assert.Equal(1, context.People.FirstOrDefault().Id);
            Assert.Equal("en-US", context.People.FirstOrDefault().Region_code);
            Assert.Equal("Don't fail plz", context.People.FirstOrDefault().Name);
        }

        [Fact]
        public void EfCoreSetup_Database_MapAfterSaveAndGet()
        {
            var context = GetDataContext;
            context.People.Add(new Person { Id = 1, Region_code = "en-US", Name = "Don't fail plz" });
            context.SaveChanges();
            var mapper = GetMapper;
            var mapped = mapper.Map<DtoPerson>(context.People.FirstOrDefault());
            Assert.NotNull(mapped);
            Assert.Equal(1, mapped.Id);
            Assert.Equal("en-US", mapped.Region_code);
            Assert.Equal("Don't fail plz", mapped.Name);
        }

        [Fact]
        public void EfCoreSetup_Database_LoadPartnerWithoutEagerLoading()
        {
            var context = GetDataContext;
            context.People.Add(new Person { Id = 1, Region_code = "en-US", Name = "first" });
            context.People.Add(new Person { Id = 2, Region_code = "en-US", Name = "second", PartnerId = 1 });
            context.SaveChanges();

            // load without include, partner is null. Need as no tracking here for it to behave like sql.
            var second = context.People.AsNoTracking().First(x => x.Id == 2);
            Assert.NotNull(second);
            Assert.Null(second.Partner);

            // load with include, parter exists.
            second = context.People.Include(x => x.Partner).First(x => x.Id == 2);
            Assert.NotNull(second);
            Assert.NotNull(second.Partner);
            Assert.Equal(1, second.Partner.Id);

            // works with string too
            second = context.People.Include("Partner").First(x => x.Id == 2);
            Assert.NotNull(second);
            Assert.NotNull(second.Partner);
            Assert.Equal(1, second.Partner.Id);
        }

        [Fact]
        public void EfCoreSetup_Database_SimpleProjection_Favorite_WithArray()
        {
            var source = new[]{
                new Person {
                    Id = 1,
                    Region_code = "en-US",
                    Name = "first",
                    Favorite = new Child {
                        Id = 20,
                        Region_code = "en-US",
                        Name = "second",
                    }
                }
            }.AsQueryable();

            // load with include, parter exists.
            var projected = source.ProjectTo<DtoPerson>(GetMapper.ConfigurationProvider, x => x.Favorite);
            var parent = projected.First();
            Assert.NotNull(parent);
            Assert.NotNull(parent.Favorite);
            Assert.Equal(20, parent.Favorite.Id);
        }


        [Fact]
        public void EfCoreSetup_Database_SimpleProjection_Child()
        {
            var context = GetDataContext;
            context.Children.Add(new Child { Id = 10, Region_code = "en-US", Name = "best child ever!" });
            context.People.Add(new Person { Id = 1, Region_code = "en-US", Name = "parent", FavoriteId = 10 });
            context.SaveChanges();

            var source = context.People.AsNoTracking().Include(x => x.Favorite);

            // load with include, parter exists.
            var projected = source.ProjectTo<DtoPerson>(GetMapper.ConfigurationProvider, x => x.Favorite);
            var parent = projected.First();
            Assert.NotNull(parent);
            Assert.NotNull(parent.Favorite);
            Assert.Equal(10, parent.Favorite.Id);
        }

        [Fact]
        public void EfCoreSetup_Database_Mapper_NoData_PeopleExpandFavorite()
        {
            var mapper = GetMapper;
            var configProvider = mapper.ConfigurationProvider;
            var context = GetDataContext;

            context.Children.Add(new Child { Id = 10, Region_code = "en-US", Name = "best child ever" });
            context.People.Add(new Person { Id = 1, Region_code = "en-US", Name = "parent", FavoriteId = 10 });
            context.SaveChanges();

            var nodata = new NoData.NoDataBuilder<DtoPerson>("Favorite");
            var source = context.People.AsNoTracking();

            var projected = nodata.Projection(source, configProvider).BuildQueryable();

            var parent = projected.First();
            Assert.NotNull(parent);
            Assert.NotNull(parent.Favorite);
            Assert.Equal(10, parent.Favorite.Id);
        }

        [Fact]
        public void EfCoreSetup_Database_Mapper_NoData_PeopleExpandFavoriteFavorite()
        {
            var mapper = GetMapper;
            var configProvider = mapper.ConfigurationProvider;
            var context = GetDataContext;

            context.GrandChildren.Add(new GrandChild { Id = 100, Region_code = "en-US", Name = "best grand child ever" });
            context.Children.Add(new Child { Id = 10, Region_code = "en-US", Name = "best child ever", FavoriteId = 100 });
            context.People.Add(new Person { Id = 1, Region_code = "en-US", Name = "parent", FavoriteId = 10 });
            context.SaveChanges();

            var nodata = new NoData.NoDataBuilder<DtoPerson>("Favorite/Favorite");
            var source = context.People.AsNoTracking();

            var projected = nodata.Projection(source, configProvider).BuildQueryable();

            var parent = projected.First();
            Assert.NotNull(parent);
            Assert.NotNull(parent.Favorite);
            Assert.Equal(10, parent.Favorite.Id);
            Assert.NotNull(parent.Favorite.Favorite);
            Assert.NotNull(parent.Favorite.Favorite);
            Assert.Equal(100, parent.Favorite.Favorite.Id);
        }
    }
}