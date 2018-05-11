using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;

namespace BinaryExpressionParserTests
{
    [TestFixture]
    public class FilterPropertyIgnoreTest
    {
        public class Dto
        {
            public int id { get; set; }
            public string Name { get; set; }
            public string region_code { get; set; }
            public Dto partner { get; set; }
            public ICollection<DtoChild> children { get; set; }
            public DtoChild favorite { get; set; }
        }

        public class DtoChild
        {
            public int id { get; set; }
            public string Name { get; set; }
            public string region_code { get; set; }
            public DtoChild partner { get; set; }
            public ICollection<DtoGrandChild> children { get; set; }
            public DtoGrandChild favorite { get; set; }
        }

        public class DtoGrandChild
        {
            public int id { get; set; }
            public string Name { get; set; }
            public string region_code { get; set; }
        }

        public static IEnumerable<DtoGrandChild> GrandChildCollection => new List<DtoGrandChild>
        {
            new DtoGrandChild{ id = 100, Name = "George German grand child 1", region_code = "de"  },
            new DtoGrandChild{ id = 200, Name = "George German grand child 2", region_code = "de"  },
        };

        public static IEnumerable<DtoChild> ChildCollection
        {
            get
            {
                var result = new List<DtoChild>
                {
                    new DtoChild{
                        id = 10,
                        Name = "John child 1",
                        region_code = "en",
                        children = GrandChildCollection.Where(x => x.id == 100 || x.id == 200).ToList()
                    },
                    new DtoChild{
                        id = 20,
                        Name = "John child 2",
                        region_code = "en",
                        children = GrandChildCollection.Where(x => x.id == 100 || x.id == 200).ToList()
                    },
                };
                result[1].partner = result[0];
                result[0].partner = result[1];
                return result;
            }
        }

        public static IEnumerable<Dto> ParentCollection
        {
            get
            {
                var result = new List<Dto>
                {
                    new Dto{
                        id = 1,
                        Name = "John",
                        region_code = "en",
                        children = ChildCollection.Where(x => x.id == 10).ToList(),
                        favorite = ChildCollection.Single(x => x.id == 10)
                    },
                    new Dto{ id = 2, Name = "Jane", region_code = "es", children = ChildCollection.Where(x => x.id == 10).ToList() },
                };
                result[1].partner = result[0];
                result[0].partner = result[1];
                return result;
            }
        }

        [Test]
        public void FilterExpand_Deserialized_SelectNameIdAndNameOfPartner_SelectInsideExpand_Success()
        {
            var filter = new NoData.NoDataQuery<Dto>("partner($filter=Name eq 'Jane')", null, "Name");
            var serialized = filter.JsonResult(ParentCollection.AsQueryable());
            Assert.NotNull(serialized);
            Assert.True(serialized.Contains("["), "is an array");
            Assert.True(serialized.Contains("]"), "is an array");

            Assert.True(serialized.Contains("Name"));
            Assert.True(serialized.Contains("John"), "Name is selected and should appear.");
            Assert.True(serialized.Contains("Jane"), "Name is selected and should appear.");

            Assert.True(serialized.Contains("id"), "id ");
            Assert.False(serialized.Contains("1"), "Selected out the top level id property.");
            Assert.True(serialized.Contains("2"), "expanded on partners, Jane's id should be present.");

            Assert.True(serialized.Contains("region_code"), "Jane has a region_code.");

            Assert.True(serialized.Contains("partner"), "Expanded on partner");
            Assert.False(serialized.Contains("children"), "not expanded on children");
            Assert.False(serialized.Contains("favorite"), "not expanded on favorite");
            Assert.False(serialized.Contains("null"), "Correlates to navigation properties that are serialized, but have a null value when they should not be serialized.");
        }


        //[Test] expand (filter;select)
        //public void FilterExpand_Deserialized_SelectNameIdAndNameOfPartner_SelectInsideExpand_Success()
        //{
        //    var filter = new NoData.NoDataQuery<Dto>("partner($filter=Name eq 'Jane')", null, "Name");
        //    var serialized = filter.JsonResult(ParentCollection.AsQueryable());
        //    Assert.NotNull(serialized);
        //    Assert.True(serialized.Contains("["), "is an array");
        //    Assert.True(serialized.Contains("]"), "is an array");

        //    Assert.True(serialized.Contains("Name"));
        //    Assert.True(serialized.Contains("John"), "Name is selected and should appear.");
        //    Assert.True(serialized.Contains("Jane"), "Name is selected and should appear.");

        //    Assert.True(serialized.Contains("id"), "id ");
        //    Assert.False(serialized.Contains("1"), "Selected out the top level id property.");
        //    Assert.True(serialized.Contains("2"), "expanded on partners, Jane's id should be present.");

        //    Assert.True(serialized.Contains("region_code"), "Jane has a region_code.");

        //    Assert.True(serialized.Contains("partner"), "Expanded on partner");
        //    Assert.False(serialized.Contains("children"), "not expanded on children");
        //    Assert.False(serialized.Contains("favorite"), "not expanded on favorite");
        //    Assert.False(serialized.Contains("null"), "Correlates to navigation properties that are serialized, but have a null value when they should not be serialized.");
        //}
    }
}
