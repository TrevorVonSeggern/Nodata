using System;
using System.Collections.Generic;
using Xunit;
using FluentAssertions;
using NoData.Tests.SharedExampleClasses;
using System.Linq;
using AutoMapper;
using NoData.Tests.SharedExampleClasses.Database.Entity;

namespace NoData.Tests.EfficiencyTests
{
    public class NPlusOneTests
    {
        private readonly IConfigurationProvider Config = SharedExampleClasses.Automapper.AutoMapperConfig.CreateConfig().ConfigurationProvider;
        private int parentCalls = 0;
        // private int childCalls = 0;
        // private int grandChildCalls = 0;

        private IEnumerable<SharedExampleClasses.Database.Entity.Person> ParentCollection()
        {
            Person GetParent()
            {
                ++parentCalls;
                return new Person
                {
                    Id = 1,
                    Region_code = "en",
                    Name = "Test class",
                    Children = new List<Child>()
                };
            }
            yield return GetParent();
        }


        public NPlusOneTests()
        {
            parentCalls = 0;
            // childCalls = 0;
            // grandChildCalls = 0;
        }


        [Fact]
        public void EfficiencyTests_NPlusOneTests_SetupIsValid_ApplyQuery()
        {
            var queryable = ParentCollection().AsQueryable();
            var nodata = new NoDataBuilder<Person>(null, null, null);
            var lst = nodata.Load(queryable).BuildQueryable().ToList();
            lst.Should().NotBeNullOrEmpty();
            lst.Should().ContainSingle();
            parentCalls.Should().Be(1);
        }

        [Fact]
        public void EfficiencyTests_NPlusOneTests_SetupIsValid_Projection()
        {
            var queryable = ParentCollection().AsQueryable();
            var nodata = new NoDataBuilder<Dto>(null, null, null);
            var projection = nodata.Projection(queryable, Config).BuildQueryable();
            var lst = projection.ToList();
            lst.Should().NotBeNullOrEmpty();
            lst.Should().ContainSingle();
            parentCalls.Should().Be(1);
        }

        [Fact]
        public void EfficiencyTests_NPlusOneTests_SetupIsValid_Json()
        {
            var queryable = ParentCollection().AsQueryable();
            var nodata = new NoDataBuilder<Person>(null, null, null);
            var json = nodata.Load(queryable).AsJson();
            json.Should().NotBeNullOrEmpty();
            parentCalls.Should().Be(1);
        }

        [Fact]
        public void EfficiencyTests_NPlusOneTests_SetupIsValid_Queryable_BuilderApply_CalledOnce()
        {
            var queryable = ParentCollection().AsQueryable();
            var nodata = new NoDataBuilder<Person>(null, null, null);
            var lst = nodata.Load(queryable).BuildQueryable().ToList();
            lst.Should().NotBeNullOrEmpty();
            parentCalls.Should().Be(1);
        }

        [Fact]
        public void EfficiencyTests_NPlusOneTests_Projection_ExactlyOneParentEnumCalled()
        {
            var queryable = ParentCollection().AsQueryable();
            var nodata = new NoDataBuilder<Dto>(null, null, null);
            var projection = nodata.Projection(queryable, Config).BuildQueryable();
            var lst = projection.ToList();
            lst.Should().NotBeNullOrEmpty();
            lst.Should().ContainSingle();
            parentCalls.Should().Be(1);
        }

        [Fact]
        public void EfficiencyTests_NPlusOneTests_Project_With_Json_ExactlyOneParentEnumCalled()
        {
            var queryable = ParentCollection().AsQueryable();
            var nodata = new NoDataBuilder<Dto>(null, null, null);
            var json = nodata.Projection(queryable, Config).AsJson();
            parentCalls.Should().Be(1);
        }
    }
}