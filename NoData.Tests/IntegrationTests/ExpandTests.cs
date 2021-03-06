using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NoData.Tests.SharedExampleClasses;
using NoData.Utility;
using Xunit;

namespace NoData.Tests.IntegrationTests
{
    public class ExpandTests
    {
        [Theory]
        [InlineData(null, 1, 2, 3, 4, 5, 6)] // returns everything.
        [InlineData("", 1, 2, 3, 4, 5, 6)] // returns everything.
        [InlineData("partner", 1, 2, 3, 4, 5, 6, 1, 2)] // one expand
        [InlineData("children", 1, 2, 3, 4, 5, 6, 10, 30, 40, 50, 60)]
        [InlineData("favorite", 1, 2, 3, 4, 5, 6, 10, 40)]
        [InlineData("favorite/favorite", 1, 2, 3, 4, 5, 6, 10, 40, 300)]
        [InlineData("partner,children", 1, 2, 3, 4, 5, 6, 1, 2, 10, 30, 40, 50, 60)] // multiple expands.
        [InlineData("children/partner", 1, 2, 3, 4, 5, 6, 10, 30, 40, 50, 60, 10, 60)]
        [InlineData("partner,children/partner", 1, 2, 3, 4, 5, 6, /*1stpartner*/1, 2, /*children*/10, 30, 40, 50, 60, /*children/partner*/60, 10)]
        [InlineData("partner/children,partner/favorite", 1, 2, 3, 4, 5, 6, /*root*/ 1, 2, /*partner*/ 10, /*partner/children*/ 10 /*partner/favorite*/ )]
        [InlineData("partner/partner", 1, 2, 3, 4, 5, 6, 1, 2, 1, 2)] // two expands
        [InlineData("partner/partner/partner", 1, 2, 3, 4, 5, 6, 1, 2, 1, 2, 1, 2)] // three expands
        [InlineData("partner/partner/partner/partner", 1, 2, 3, 4, 5, 6, 1, 2, 1, 2, 1, 2, 1, 2)] // four expands
        [InlineData("partner($expand=partner)", 1, 2, 3, 4, 5, 6, 1, 2, 1, 2)] // nested expand syntax
        [InlineData("partner($expand=partner($expand=partner))", 1, 2, 3, 4, 5, 6, 1, 2, 1, 2, 1, 2)]
        [InlineData("partner($expand=partner($expand=partner($expand=partner)))", 1, 2, 3, 4, 5, 6, 1, 2, 1, 2, 1, 2, 1, 2)] // now your getting crazy...
        [InlineData("partner($expand=partner($expand=partner($expand=partner;$filter=id eq 1)))", 2, 1, 2, 1, 2)]
        [InlineData("partner($expand=partner;$filter=id eq 1)", 2, 1, 2)]
        [InlineData("partner($expand=partner($expand=partner($expand=partner;$filter=id eq 1;$select=id)))", 2, 1, 2, 1, 2)]
        [InlineData("partner($expand=partner;$filter=id eq 1;$select=id)", 2, 1, 2)]
        [InlineData("partner($expand=partner($expand=partner($select=id;$expand=partner;$filter=id eq 1)))", 2, 1, 2, 1, 2)]
        [InlineData("partner($select=id;$expand=partner;$filter=id eq 1)", 2, 1, 2)]
        [InlineData("partner($select=id,Name;$expand=partner($select=id,region_code;$expand=partner($select=id;$select=id;$expand=partner;$filter=id eq 1)))", 2, 1, 2, 1, 2)]
        public void Expand_Expression(string expression, params int[] expectedIds)
        {
            var queryable = new List<Dto>(ParentCollection).AsQueryable();
            DefaultSettingsForType<Dto>.SettingsForType.MaxExpandDepth = 100;
            var ft = new NoData.NoDataBuilder<Dto>(new Parameters(expression), DefaultSettingsForType<Dto>.SettingsForType);
            var result = ft.Load(queryable).BuildQueryable().ToList();

            var resultIds = result.SelectMany(x => x.GetAllIds()).OrderBy(x => x).ToList();
            var expected = expectedIds.OrderBy(x => x).ToList();

            Assert.NotNull(result);

            resultIds.Should().BeEquivalentTo(expected, "Error with expression: " + expression);
        }

        [Fact]
        public void Expand_WithExpandLimit_0_Fails()
        {
            var queryable = new List<Dto>(ParentCollection).AsQueryable();
            var ft = new NoData.NoDataBuilder<Dto>(new Parameters(nameof(Dto.partner)), new SettingsForType<Dto>() { MaxExpandDepth = 0 });

            Assert.Throws<ArgumentException>(() =>
            {
                ft.Load(queryable);
            });
        }

        [Fact]
        public void Expand_WithExpandLimit_1_Success()
        {
            var queryable = new List<Dto>(ParentCollection).AsQueryable();
            var ft = new NoData.NoDataBuilder<Dto>(new Parameters(nameof(Dto.partner)), new SettingsForType<Dto>() { MaxExpandDepth = 1 });

            ft.Load(queryable);
        }

        [Fact]
        public void Expand_WithExpandLimit_2_Success()
        {
            var queryable = new List<Dto>(ParentCollection).AsQueryable();
            var ft = new NoData.NoDataBuilder<Dto>(new Parameters(nameof(Dto.partner)), new SettingsForType<Dto>() { MaxExpandDepth = 2 });

            ft.Load(queryable);
        }

        [Fact]
        public void Expand_WithExpandLimit_Unset_Success()
        {
            var queryable = new List<Dto>(ParentCollection).AsQueryable();
            var ft = new NoData.NoDataBuilder<Dto>(new Parameters(nameof(Dto.partner)), new SettingsForType<Dto>());

            ft.Load(queryable);
        }

        public static IEnumerable<DtoGrandChild> GrandChildCollection => new List<DtoGrandChild>
        {
            new DtoGrandChild{ id = 100, Name = "George German grand child 1", region_code = "de"  },
            new DtoGrandChild{ id = 200, Name = "George German grand child 2", region_code = "de"  },
            new DtoGrandChild{ id = 300, Name = "George US grand child 1", region_code = "en"  },
            new DtoGrandChild{ id = 400, Name = "George US grand child 2", region_code = "en"  },
            new DtoGrandChild{ id = 500, Name = "George Mexican grand child 1", region_code = "es"  },
            new DtoGrandChild{ id = 600, Name = "George Mexican grand child 2", region_code = "es"  },
        };

        public static IEnumerable<DtoChild> ChildCollection
        {
            get
            {
                var result = new List<DtoChild>
                {
                    new DtoChild{ id = 10, Name = "John child 1", region_code = "en" },
                    new DtoChild{ id = 30, Name = "George child 1", region_code = "de", children = GrandChildCollection.Where(x => x.id == 100 || x.id == 200).ToList()},
                    new DtoChild{
                        id = 40,
                        Name = "George child 2",
                        region_code = "es",
                        children = GrandChildCollection.Where(x => x.id == 300 || x.id == 400).ToList(),
                        favorite = GrandChildCollection.Single(x => x.id == 300)
                    },
                    new DtoChild{ id = 50, Name = "George child 3", region_code = "en", children = GrandChildCollection.Where(x => x.id == 500 || x.id == 600).ToList()  },
                    new DtoChild{ id = 60, Name = "Joe child 1", region_code = "en" },
                };
                result[4].partner = result[0];
                result[0].partner = result[4];
                return result;
            }
        }

        public static IEnumerable<Dto> ParentCollection
        {
            get
            {
                var result = new List<Dto>
                {
                    new Dto{ id = 1, Name = "John", region_code = "en", children = ChildCollection.Where(x => x.id == 10).ToList() },
                    new Dto{ id = 2, Name = "John", region_code = "en", favorite = ChildCollection.Single(x => x.id == 10) },
                    new Dto{ id = 3, Name = "George", region_code = "de", children = ChildCollection.Where(x => x.id == 30).ToList() },
                    new Dto{ id = 4, Name = "George", region_code = "en", children = ChildCollection.Where(x => x.id == 40).ToList(), favorite = ChildCollection.Single(x => x.id == 40) },
                    new Dto{ id = 5, Name = "George", region_code = "es", children = ChildCollection.Where(x => x.id == 50).ToList() },
                    new Dto{ id = 6, Name = "Joe", region_code = "en", children = ChildCollection.Where(x => x.id == 60).ToList() },
                };
                result[1].partner = result[0];
                result[0].partner = result[1];
                return result;
            }
        }

    }
}
