using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;

namespace BinaryExpressionParserTests
{
    [TestFixture]
    public class PropertyIgnoreTest
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
                    new Dto{ id = 2, Name = "Jane", region_code = "en", children = ChildCollection.Where(x => x.id == 10).ToList() },
                };
                result[1].partner = result[0];
                result[0].partner = result[1];
                return result;
            }
        }

        [Test]
        public void Expand_Ignore_Deserialized_Success()
        {
            var filter = new NoData.NoDataQuery<Dto>();
            var serialized = filter.JsonResult(ParentCollection.AsQueryable());
            Assert.NotNull(serialized);
            Assert.True(serialized.Contains("["), "is array");// returns list
            Assert.True(serialized.Contains("id")); // basic properties
            Assert.True(serialized.Contains("Name"));
            Assert.True(serialized.Contains("region_code"));
            Assert.True(serialized.Contains("1"));
            Assert.True(serialized.Contains("John"));
            Assert.True(serialized.Contains("en")); // basic values
            Assert.False(serialized.Contains("children")); // ignored values
            Assert.False(serialized.Contains("partner"));
            Assert.False(serialized.Contains("favorite"));
        }

        [Test]
        public void Expand_Ignore_Deserialized_ExpandPartner_Success()
        {
            var filter = new NoData.NoDataQuery<Dto>("partner", null, null);
            var serialized = filter.JsonResult(ParentCollection.AsQueryable());
            Assert.NotNull(serialized);
            Assert.True(serialized.Contains("["), "is array");// returns list
            Assert.True(serialized.Contains("id")); // basic properties
            Assert.True(serialized.Contains("Name"));
            Assert.True(serialized.Contains("region_code"));
            Assert.True(serialized.Contains("1"));
            Assert.True(serialized.Contains("2"));
            Assert.True(serialized.Contains("John"));
            Assert.True(serialized.Contains("Jane"));
            Assert.True(serialized.Contains("partner"));
            Assert.True(serialized.Contains("en")); // basic values
            Assert.False(serialized.Contains("children")); // ignored values
            Assert.False(serialized.Contains("favorite"));
            Assert.False(serialized.Contains("null"));
        }

        [Test]
        public void Expand_Ignore_Deserialized_ExpandChildren_Success()
        {
            var filter = new NoData.NoDataQuery<Dto>("children", null, null);
            var serialized = filter.JsonResult(ParentCollection.AsQueryable());
            Assert.NotNull(serialized);
            Assert.True(serialized.Contains("["), "is array");// returns list
            Assert.True(serialized.Contains("id")); // basic properties
            Assert.True(serialized.Contains("Name"));
            Assert.True(serialized.Contains("region_code"));
            Assert.True(serialized.Contains("1"));
            Assert.True(serialized.Contains("John"));
            Assert.True(serialized.Contains("child 1"));
            Assert.True(serialized.Contains("en")); // basic values
            Assert.False(serialized.Contains("partner")); // ignored values
            Assert.False(serialized.Contains("favorite"));
            Assert.False(serialized.Contains("null"));
        }

        [Test]
        public void Expand_Ignore_Deserialized_ExpandChildrenOfChildren_Success()
        {
            var filter = new NoData.NoDataQuery<Dto>("children/children", null, null);
            var serialized = filter.JsonResult(ParentCollection.AsQueryable());
            Assert.NotNull(serialized);
            Assert.True(serialized.Contains("["), "is array");// returns list
            Assert.True(serialized.Contains("id")); // basic properties
            Assert.True(serialized.Contains("Name"));
            Assert.True(serialized.Contains("region_code"));
            Assert.True(serialized.Contains("1"));
            Assert.True(serialized.Contains("John"));
            Assert.True(serialized.Contains("child 1"));
            Assert.True(serialized.Contains("en")); // basic values
            Assert.True(serialized.Contains("100")); // children of children
            Assert.True(serialized.Contains("200"));
            Assert.False(serialized.Contains("partner")); // ignored values
            Assert.False(serialized.Contains("favorite"));
            Assert.False(serialized.Contains("null"));
        }


        [Test]
        public void Expand_Ignore_Deserialized_ExpandPartnerPartner_Success()
        {
            var filter = new NoData.NoDataQuery<Dto>("partner/partner", null, null);
            var serialized = filter.JsonResult(ParentCollection.AsQueryable());
            Assert.NotNull(serialized);
            Assert.True(serialized.Contains("["), "is array");// returns list
            Assert.True(serialized.Contains("id")); // basic properties
            Assert.True(serialized.Contains("Name"));
            Assert.True(serialized.Contains("region_code"));
            Assert.True(serialized.Contains("1"));
            Assert.True(serialized.Contains("2"));
            // should appear three times. Once for the root. Once for the partner, once for the partners partner.
            Assert.AreEqual(3, serialized.Count(x => x == '1'));
            Assert.AreEqual(3, serialized.Count(x => x == '2'));
            Assert.True(serialized.Contains("John"));
            Assert.True(serialized.Contains("Jane"));
            Assert.True(serialized.Contains("partner"));
            Assert.True(serialized.Contains("en")); // basic values
            Assert.False(serialized.Contains("children")); // ignored values
            Assert.False(serialized.Contains("favorite"));
            Assert.False(serialized.Contains("null"));
        }
    }
}
