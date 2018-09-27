using System.Linq;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using AutoMapper.QueryableExtensions;
using System.Collections.Generic;
using Xunit;
using FluentAssertions;
using System;

namespace NoData.Tests.EFCoreSetupTest
{
    public class NonsenseProjectionTests
    {
        private DataContext GetDataContext => DataContext.GetInMemoryDatabase();
        private IMapper GetMapper => AutoMapperConfig.CreateConfig();

        class NonsenseThingOneEntity
        {
            public int Id { get; set; }
            public int bId { get; set; }
            public int cId { get; set; }
            public NonsenseThingChildEntity B { get; set; }
            public NonsenseThingChildEntity C { get; set; }
        }
        class NonsenseThingOneDto
        {
            public int Id { get; set; }
            public int aId { get; set; }
            public int bId { get; set; }
            public NonsenseThingChildDto A { get; set; } // Maps to entity property B
            public NonsenseThingChildDto B { get; set; } // Maps to entity property C
        }

        class NonsenseThingChildEntity
        {
            public int Id { get; set; }
        }
        class NonsenseThingChildDto
        {
            public int Id { get; set; }
        }

        [Fact]
        public void EfCoreSetup_Configuration_IsValid()
        {
            var mapper = GetMapper;
            mapper.ConfigurationProvider.AssertConfigurationIsValid();

            var context = GetDataContext;

            context.Thing.Add(new NonsenseThingOneEntity
            {
                bId = 1,
                cId = 2,
                B = new NonsenseThingChildEntity { Id = 1 },
                C = new NonsenseThingChildEntity { Id = 2 },
            });
            context.SaveChanges();

            var thingList = context.Thing.AsNoTracking().ToList();

            // Assert on thing
            thingList.Should().ContainSingle();
            var thing = thingList.Single();
            thing.bId.Should().Be(1);
            thing.cId.Should().Be(2);
            Assert.Null(thing.B);
            Assert.Null(thing.C);

            // Assert on a child from the list.
            var childList = context.Child.AsNoTracking().ToList();
            childList.Should().HaveCount(2);

            // Assert on a child from the include            
            var thingIncludeList = context.Thing.AsNoTracking().Include(x => x.B).ToList();
            thingIncludeList.Should().ContainSingle();
            var bChildThing = thingIncludeList.Single().B;
            bChildThing.Should().NotBeNull();
            bChildThing.Id.Should().Be(1);
            Assert.Null(thingIncludeList.Single().C);

            mapper.Map<NonsenseThingOneDto>(thingIncludeList.Single());
            mapper.Map<NonsenseThingChildDto>(bChildThing);
        }

        [Fact]
        public void EfCoreSetup_Projection_WithNonsenseMaps_NoExpand()
        {
            var context = GetDataContext;
            var mapper = GetMapper;
            var mapConfig = mapper.ConfigurationProvider;

            context.Thing.Add(new NonsenseThingOneEntity
            {
                bId = 1,
                cId = 2,
                B = new NonsenseThingChildEntity { Id = 1 },
                C = new NonsenseThingChildEntity { Id = 2 },
            });
            context.SaveChanges();

            var thingSource = context.Thing.AsNoTracking();

            var nodata = new NoDataBuilder<NonsenseThingOneDto>(null, null, null);

            var dest = nodata.Projection<NonsenseThingOneEntity>(thingSource, mapConfig).BuildQueryable().ToList();
            dest.Should().ContainSingle();
            var dto = dest.Single();
            dto.Should().NotBeNull();
            Assert.Null(dto.A);
            Assert.Null(dto.B);
            dto.aId.Should().Be(1);
            dto.bId.Should().Be(2);
        }

        [Fact]
        public void EfCoreSetup_Projection_WithNonsenseMaps_ExpandAB()
        {
            var context = GetDataContext;
            var mapper = GetMapper;
            var mapConfig = mapper.ConfigurationProvider;

            context.Thing.Add(new NonsenseThingOneEntity
            {
                bId = 1,
                cId = 2,
                B = new NonsenseThingChildEntity { Id = 1 },
                C = new NonsenseThingChildEntity { Id = 2 },
            });
            context.SaveChanges();

            var thingSource = context.Thing.AsNoTracking();

            var nodata = new NoDataBuilder<NonsenseThingOneDto>("A,B", null, null);

            var dest = nodata.Projection<NonsenseThingOneEntity>(thingSource, mapConfig).BuildQueryable().ToList();
            dest.Should().ContainSingle();
            var dto = dest.Single();
            dto.Should().NotBeNull();
            Assert.NotNull(dto.A);
            Assert.NotNull(dto.B);
            dto.aId.Should().Be(1);
            dto.bId.Should().Be(2);
            dto.A.Id.Should().Be(1);
            dto.B.Id.Should().Be(2);
        }

        [Fact]
        public void EfCoreSetup_Projection_WithNonsenseMaps_ExpandA_Only()
        {
            var context = GetDataContext;
            var mapper = GetMapper;
            var mapConfig = mapper.ConfigurationProvider;

            context.Thing.Add(new NonsenseThingOneEntity
            {
                bId = 1,
                cId = 2,
                B = new NonsenseThingChildEntity { Id = 1 },
                C = new NonsenseThingChildEntity { Id = 2 },
            });
            context.SaveChanges();

            var thingSource = context.Thing.AsNoTracking();

            var nodata = new NoDataBuilder<NonsenseThingOneDto>("A", null, null);

            var dest = nodata.Projection<NonsenseThingOneEntity>(thingSource, mapConfig).BuildQueryable().ToList();
            dest.Should().ContainSingle();
            var dto = dest.Single();
            dto.Should().NotBeNull();
            Assert.NotNull(dto.A);
            Assert.Null(dto.B);
            dto.aId.Should().Be(1);
            dto.bId.Should().Be(2);
            dto.A.Id.Should().Be(1);
        }


        class AutoMapperConfig
        {
            internal static IMapper CreateConfig()
            {
                var mapConfig = new MapperConfiguration(config =>
                {
                    config.CreateMap<NonsenseThingOneEntity, NonsenseThingOneDto>()
                        .ForMember(x => x.aId, opt => opt.MapFrom(d => d.bId))
                        .ForMember(x => x.bId, opt => opt.MapFrom(d => d.cId))
                        .ForMember(x => x.A, opt => { opt.MapFrom(d => d.B); opt.ExplicitExpansion(); })
                        .ForMember(x => x.B, opt => { opt.MapFrom(d => d.C); opt.ExplicitExpansion(); })
                        ;
                    config.CreateMap<NonsenseThingChildEntity, NonsenseThingChildDto>();
                });

                mapConfig.AssertConfigurationIsValid();

                return new Mapper(mapConfig);
            }
        }

        class DataContext : DbContext
        {
            public static DataContext GetInMemoryDatabase()
            {
                var databaseOptions = new DbContextOptionsBuilder<DataContext>()
                    .UseInMemoryDatabase(databaseName: System.Guid.NewGuid().ToString())
                    .Options;
                return new DataContext(databaseOptions);
            }

            public DataContext(DbContextOptions<DataContext> options) : base(options)
            {
            }

            public DbSet<NonsenseThingOneEntity> Thing { get; set; }
            public DbSet<NonsenseThingChildEntity> Child { get; set; }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.Entity<NonsenseThingOneEntity>().HasOne(x => x.B);
                modelBuilder.Entity<NonsenseThingOneEntity>().HasOne(x => x.C);
            }
        }
    }
}