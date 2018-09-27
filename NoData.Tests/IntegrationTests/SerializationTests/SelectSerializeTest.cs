using Xunit;
using System.Collections.Generic;
using System.Linq;
using NoData.Tests.SharedExampleClasses;

namespace NoData.Tests.SelectTests
{
    public class SelectPropertyIgnoreTest
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
                    new Dto{ id = 2, Name = "Jane", region_code = "es", children = ChildCollection.Where(x => x.id == 10).ToList() },
                };
                result[1].partner = result[0];
                result[0].partner = result[1];
                return result;
            }
        }

        public class DtoWithNullPartnerId
        {
            public int id { get; set; }
            public string Name { get; set; }
            public string region_code { get; set; }
            public int? partnerId { get; set; }
        }

        [Fact]
        public void Select_Deserialized_Null_PartnerIdExists_Success()
        {
            var filter = new NoData.NoDataBuilder<DtoWithNullPartnerId>(null, null, null);
            var input = new[] { new DtoWithNullPartnerId { partnerId = 1 } };
            var serialized = filter.Load(input.AsQueryable()).AsJson();
            Assert.NotNull(serialized);
            Assert.Contains("[", serialized);
            Assert.Contains("]", serialized);

            Assert.Contains("1", serialized);
        }

        [Fact]
        public void Select_Deserialized_SelectName_Success()
        {
            var filter = new NoData.NoDataBuilder<Dto>(null, null, "Name");
            var serialized = filter.Load(ParentCollection.AsQueryable()).AsJson();
            Assert.NotNull(serialized);
            Assert.Contains("[", serialized);
            Assert.Contains("]", serialized);

            Assert.Contains("Name", serialized);
            Assert.Contains("John", serialized);
            Assert.Contains("Jane", serialized);

            Assert.DoesNotContain("id", serialized);
            Assert.DoesNotContain("1", serialized);
            Assert.DoesNotContain("2", serialized);

            Assert.DoesNotContain("region_code", serialized);
            Assert.DoesNotContain("en", serialized);
            Assert.DoesNotContain("en", serialized);

            Assert.DoesNotContain("partner", serialized);
            Assert.DoesNotContain("children", serialized);
            Assert.DoesNotContain("favorite", serialized);
            Assert.DoesNotContain("null", serialized);
        }

        [Fact]
        public void SelectExpand_Deserialized_ExpandPartnerSelectId_Success()
        {
            var filter = new NoData.NoDataBuilder<Dto>("partner", null, "id");
            var serialized = filter.Load(ParentCollection.AsQueryable()).AsJson();
            Assert.NotNull(serialized);
            Assert.Contains("[", serialized);
            Assert.Contains("]", serialized);

            Assert.Contains("Name", serialized);
            Assert.Contains("John", serialized);
            Assert.Contains("Jane", serialized);

            Assert.Contains("id", serialized);
            Assert.Contains("1", serialized);
            Assert.Contains("2", serialized);
            Assert.DoesNotContain("10", serialized);

            Assert.Contains("partner", serialized);
            Assert.DoesNotContain("children", serialized);
            Assert.DoesNotContain("favorite", serialized);
            Assert.DoesNotContain("null", serialized);
        }

        [Fact]
        public void SelectExpand_Deserialized_ExpandChildren_Success()
        {
            var filter = new NoData.NoDataBuilder<Dto>("children", null, "id");
            var serialized = filter.Load(ParentCollection.AsQueryable()).AsJson();
            Assert.NotNull(serialized);
            Assert.Contains("[", serialized);
            Assert.Contains("]", serialized);

            Assert.Contains("Name", serialized);
            Assert.DoesNotContain(serialized, ("\"John\""));
            Assert.DoesNotContain(serialized, ("\"Jane\""));

            Assert.Contains("id", serialized);
            Assert.Contains("1", serialized);
            Assert.Contains("2", serialized);
            Assert.Contains("10", serialized);

            Assert.Contains("children", serialized);
            Assert.DoesNotContain("partner", serialized);
            Assert.DoesNotContain("favorite", serialized);
            Assert.DoesNotContain("null", serialized);
        }


        [Fact]
        public void SelectExpand_Deserialized_SelectNameAndNameOfPartner_Success()
        {
            var filter = new NoData.NoDataBuilder<Dto>("partner", null, "Name,partner/Name");
            var serialized = filter.Load(ParentCollection.AsQueryable()).AsJson();
            Assert.NotNull(serialized);
            Assert.Contains("[", serialized);
            Assert.Contains("]", serialized);

            Assert.Contains("Name", serialized);
            Assert.Contains("John", serialized);
            Assert.Contains("Jane", serialized);

            Assert.DoesNotContain("id", serialized);
            Assert.DoesNotContain("1", serialized);
            Assert.DoesNotContain("2", serialized);

            Assert.DoesNotContain("region_code", serialized);
            Assert.DoesNotContain("en", serialized);
            Assert.DoesNotContain("en", serialized);

            Assert.Contains("partner", serialized);
            Assert.DoesNotContain("children", serialized);
            Assert.DoesNotContain("favorite", serialized);
            Assert.DoesNotContain("null", serialized);
        }

        [Fact]
        public void SelectExpand_Deserialized_SelectNameAndNameOfPartner_SelectInsideExpand_Success()
        {
            var filter = new NoData.NoDataBuilder<Dto>("partner($select=Name)", null, "Name");
            var serialized = filter.Load(ParentCollection.AsQueryable()).AsJson();
            Assert.NotNull(serialized);
            Assert.Contains("[", serialized);
            Assert.Contains("]", serialized);

            Assert.Contains("Name", serialized);
            Assert.Contains("John", serialized);
            Assert.Contains("Jane", serialized);

            Assert.DoesNotContain("id", serialized);
            Assert.DoesNotContain("1", serialized);
            Assert.DoesNotContain("2", serialized);

            Assert.DoesNotContain("region_code", serialized);
            Assert.DoesNotContain("en", serialized);
            Assert.DoesNotContain("en", serialized);

            Assert.Contains("partner", serialized);
            Assert.DoesNotContain("children", serialized);
            Assert.DoesNotContain("favorite", serialized);
            Assert.DoesNotContain("null", serialized);
        }

        [Fact]
        public void SelectExpand_Deserialized_SelectNameIdAndNameOfPartner_SelectInsideExpand_Success()
        {
            var filter = new NoData.NoDataBuilder<Dto>("partner($select=Name,id)", null, "Name,id");
            var serialized = filter.Load(ParentCollection.AsQueryable()).AsJson();
            Assert.NotNull(serialized);
            Assert.Contains("[", serialized);
            Assert.Contains("]", serialized);

            Assert.Contains("Name", serialized);
            Assert.Contains("John", serialized);
            Assert.Contains("Jane", serialized);

            Assert.Contains("id", serialized);
            Assert.Contains("1", serialized);
            Assert.Contains("2", serialized);

            Assert.DoesNotContain("region_code", serialized);
            Assert.DoesNotContain("en", serialized);
            Assert.DoesNotContain("en", serialized);

            Assert.Contains("partner", serialized);
            Assert.DoesNotContain("children", serialized);
            Assert.DoesNotContain("favorite", serialized);
            Assert.DoesNotContain("null", serialized);
        }
    }
}
