using Xunit;
using System.Collections.Generic;
using System.Linq;
using NoData.Tests.SharedExampleClasses;

namespace NoData.Tests.SelectTests
{
    public class SelectTest
    {
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

        [Fact]
        public void Select_Blank_AllPropertiesArePresent()
        {
            var ft = new NoData.NoDataQueryBuilder<Dto>(null, null, null);
            var result = ft.ApplyQueryable(new List<Dto>(ParentCollection).AsQueryable());

            var resultIds = result.SelectMany(x => x.GetAllIds()).ToList();

            Assert.NotNull(result);
            int[] expectedIds = new[] { 1, 2, 3, 4, 5, 6 };
            Assert.Equal(expectedIds.Length, resultIds.Count);
            foreach (var resultId in resultIds)
                Assert.Contains(resultId, expectedIds);

            foreach (var dto in result)
            {
                Assert.NotNull(dto);
                Assert.NotNull(dto.Name);
                Assert.NotNull(dto.region_code);
                Assert.Null(dto.favorite ?? dto.children?.FirstOrDefault());
            }
        }

        [Fact]
        public void Select_id_OnlyReturnsId_Success()
        {
            var ft = new NoData.NoDataQueryBuilder<Dto>(null, null, "id");
            var result = ft.ApplyQueryable(new List<Dto>(ParentCollection).AsQueryable());

            var resultIds = result.SelectMany(x => x.GetAllIds()).ToList();

            Assert.NotNull(result);
            int[] expectedIds = new[] { 1, 2, 3, 4, 5, 6 };
            Assert.Equal(expectedIds.Length, resultIds.Count);
            foreach (var resultId in resultIds)
                Assert.Contains(resultId, expectedIds);

            foreach (var dto in result)
            {
                Assert.NotNull(dto);
                Assert.Null(dto.Name);
                Assert.Null(dto.region_code);
                Assert.Null(dto.favorite ?? dto.children?.FirstOrDefault());
            }
        }

        [Fact]
        public void Select_Name_OnlyReturnsName_Success()
        {
            var ft = new NoData.NoDataQueryBuilder<Dto>(null, null, "Name");
            var result = ft.ApplyQueryable(new List<Dto>(ParentCollection).AsQueryable());

            var resultIds = result.SelectMany(x => x.GetAllIds()).ToList();

            Assert.NotNull(result);
            Assert.Equal(ParentCollection.Count(), resultIds.Count);

            foreach (var dto in result)
            {
                Assert.NotNull(dto);
                Assert.Equal(0, dto.id); // int cannot be null, defaults to 0 when not selected.
                Assert.NotNull(dto.Name);
                Assert.Null(dto.region_code);
                Assert.Null(dto.favorite);
                Assert.Null(dto.children);
            }
        }
    }
}
