using System.Collections.Generic;
using System.Linq;
using NoData.Tests.SharedExampleClasses;
using Xunit;

namespace NoData.Tests.ExpandTests
{
    public class PropertyIgnoreTest
    {
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

        [Fact]
        public void Expand_Ignore_Deserialized_Success()
        {
            var filter = new NoData.NoDataQueryBuilder<Dto>(null, null, null);
            var serialized = filter.JsonResult(ParentCollection.AsQueryable());
            Assert.NotNull(serialized);
            Assert.Contains("[", serialized);// returns list
            Assert.Contains("id", serialized); // basic properties
            Assert.Contains("Name", serialized);
            Assert.Contains("region_code", serialized);
            Assert.Contains("1", serialized);
            Assert.Contains("John", serialized);
            Assert.Contains("en", serialized); // basic values
            Assert.DoesNotContain("children", serialized); // ignored values
            Assert.DoesNotContain("partner", serialized);
            Assert.DoesNotContain("favorite", serialized);
        }

        [Fact]
        public void Expand_Ignore_Deserialized_ExpandPartner_Success()
        {
            var filter = new NoData.NoDataQueryBuilder<Dto>("partner", null, null);
            var serialized = filter.JsonResult(ParentCollection.AsQueryable());
            Assert.NotNull(serialized);
            Assert.Contains("[", serialized);// returns list
            Assert.Contains("id", serialized); // basic properties
            Assert.Contains("Name", serialized);
            Assert.Contains("region_code", serialized);
            Assert.Contains("1", serialized);
            Assert.Contains("2", serialized);
            Assert.Contains("John", serialized);
            Assert.Contains("Jane", serialized);
            Assert.Contains("partner", serialized);
            Assert.Contains("en", serialized); // basic values
            Assert.DoesNotContain("children", serialized); // ignored values
            Assert.DoesNotContain("favorite", serialized);
            Assert.DoesNotContain("null", serialized);
        }

        [Fact]
        public void Expand_Ignore_Deserialized_ExpandChildren_Success()
        {
            var filter = new NoData.NoDataQueryBuilder<Dto>("children", null, null);
            var serialized = filter.JsonResult(ParentCollection.AsQueryable());
            Assert.NotNull(serialized);
            Assert.Contains("[", serialized);// returns list
            Assert.Contains("id", serialized); // basic properties
            Assert.Contains("Name", serialized);
            Assert.Contains("region_code", serialized);
            Assert.Contains("1", serialized);
            Assert.Contains("John", serialized);
            Assert.Contains("child 1", serialized);
            Assert.Contains("en", serialized); // basic values
            Assert.DoesNotContain("partner", serialized); // ignored values
            Assert.DoesNotContain("favorite", serialized);
            Assert.DoesNotContain("null", serialized);
        }

        [Fact]
        public void Expand_Ignore_Deserialized_ExpandChildrenOfChildren_Success()
        {
            var filter = new NoData.NoDataQueryBuilder<Dto>("children/children", null, null);
            var serialized = filter.JsonResult(ParentCollection.AsQueryable());
            Assert.NotNull(serialized);
            Assert.Contains("[", serialized);// returns list
            Assert.Contains("id", serialized); // basic properties
            Assert.Contains("Name", serialized);
            Assert.Contains("region_code", serialized);
            Assert.Contains("1", serialized);
            Assert.Contains("John", serialized);
            Assert.Contains("child 1", serialized);
            Assert.Contains("en", serialized); // basic values
            Assert.Contains("100", serialized); // children of children
            Assert.Contains("200", serialized);
            Assert.DoesNotContain("partner", serialized); // ignored values
            Assert.DoesNotContain("favorite", serialized);
            Assert.DoesNotContain("null", serialized);
        }


        [Fact]
        public void Expand_Ignore_Deserialized_ExpandPartnerPartner_Success()
        {
            var filter = new NoData.NoDataQueryBuilder<Dto>("partner/partner", null, null);
            var serialized = filter.JsonResult(ParentCollection.AsQueryable());
            Assert.NotNull(serialized);
            Assert.Contains("[", serialized);// returns list
            Assert.Contains("id", serialized); // basic properties
            Assert.Contains("Name", serialized);
            Assert.Contains("region_code", serialized);
            Assert.Contains("1", serialized);
            Assert.Contains("2", serialized);
            // should appear three times. Once for the root. Once for the partner, once for the partners partner.
            Assert.Equal(3, serialized.Count(x => x == '1'));
            Assert.Equal(3, serialized.Count(x => x == '2'));
            Assert.Contains("John", serialized);
            Assert.Contains("Jane", serialized);
            Assert.Contains("partner", serialized);
            Assert.Contains("en", serialized); // basic values
            Assert.DoesNotContain("children", serialized); // ignored values
            Assert.DoesNotContain("favorite", serialized);
            Assert.DoesNotContain("null", serialized);
        }

        [Fact]
        public void Expand_Ignore_Deserialized_ExpandPartnerPartner_SelectId_Success()
        {
            var filter = new NoData.NoDataQueryBuilder<Dto>("partner/partner($select=id)", null, null);
            var serialized = filter.JsonResult(ParentCollection.AsQueryable());
            Assert.NotNull(serialized);
            Assert.Contains("[", serialized);// returns list
            Assert.Contains("id", serialized); // basic properties
            Assert.Contains("Name", serialized);
            Assert.Contains("region_code", serialized);
            Assert.Contains("1", serialized);
            Assert.Contains("2", serialized);
            // should appear three times. Once for the root. Once for the partner, once for the partners partner.
            Assert.Equal(3, serialized.Count(x => x == '1'));
            Assert.Equal(3, serialized.Count(x => x == '2'));
            Assert.Equal(4, serialized.Count(x => x == 'J')); // two for John, two for Jane (for root properties and on not on expanded.)
            Assert.Contains("John", serialized);
            Assert.Contains("Jane", serialized);
            Assert.Contains("partner", serialized);
            Assert.Contains("en", serialized); // basic values
            Assert.DoesNotContain("children", serialized); // ignored values
            Assert.DoesNotContain("favorite", serialized);
            Assert.DoesNotContain("null", serialized);
        }
    }
}
