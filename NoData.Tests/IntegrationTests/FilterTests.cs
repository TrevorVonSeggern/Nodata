using System;
using System.Collections.Generic;
using System.Linq;
using NoData.Tests.SharedExampleClasses;
using Xunit;

namespace NoData.Tests.IntegrationTests
{

    public class FilterTests
    {
        public static IEnumerable<Dto> SampleCollection
        {
            get
            {
                var result = new List<Dto>
                {
                    new Dto{ id = 1, Name = "John", region_code = "en", partner = new Dto{ id = 10, Name = "child", region_code = "en" } },
                    new Dto{ id = 2, Name = "John", region_code = "en"  },
                    new Dto{ id = 3, Name = "George", region_code = "de"  },
                    new Dto{ id = 4, Name = "George", region_code = "en"  },
                    new Dto{ id = 5, Name = "George", region_code = "es"  },
                    new Dto{ id = 6, Name = "Joe", region_code = "en"  },
                };
                result[0].partner.partner = result[0];
                return result;
            }
        }

        [Theory]
        [InlineData("id eq 2", 2)] // id comared to numbers.
        [InlineData("id gt 2", 3, 4, 5, 6)]
        [InlineData("id ge 2", 2, 3, 4, 5, 6)]
        [InlineData("id lt 2", 1, 1, 10)]
        [InlineData("id le 2", 1, 1, 2, 10)]
        [InlineData("id ne 2", 1, 1, 3, 4, 5, 6, 10)]
        [InlineData("Name eq 'George'", 3, 4, 5)] // Name compared to strings.
        [InlineData("Name ne 'George'", 1, 1, 2, 6, 10)]
        [InlineData("id gt 3 and Name eq 'George'", 4, 5)] // conjunctions with logic.
        [InlineData("region_code ne 'en' and Name eq 'George'", 3, 5)]
        [InlineData("region_code eq 'de' and Name eq 'George'", 3)]
        [InlineData("(region_code eq 'de' or region_code eq 'es')and Name eq 'George'", 3, 5)] // space doesn't matter(tight).
        [InlineData("   ( region_code eq 'de' or region_code eq 'es'    ) and Name eq 'George'", 3, 5)] // space doesn't matter  (  loose  )   .
        [InlineData("Name eq 'George' or (region_code eq 'en' and id eq 4 ) ", 3, 4, 5)] // grouping order matters.
        [InlineData("(Name eq 'George' or region_code eq 'en') and id eq 4", 4)]
        [InlineData("id le 1 or id ge 1", 1, 1, 2, 3, 4, 5, 6, 10)] // multiple of the same property.
        [InlineData("id le 1 and id ge 1", 1, 1, 10)]
        [InlineData("id eq 1 or id eq 1 and id eq 1 or id eq 1 and id eq 1 or id eq 1", 1, 1, 10)] // duplication doesn't matter.
        [InlineData("partner/id eq 10", 1, 1, 10)] // filter on expansion
        [InlineData("partner/partner/id eq 1", 1, 1, 10)] // filter on expansion on expansion
        [InlineData("endswith(Name,'eorge')", 3, 4, 5)] // string operators
        [InlineData("endswith(Name,'George')", 3, 4, 5)]
        [InlineData("startswith(Name,'Geo')", 3, 4, 5)]
        [InlineData("startswith(Name,'George')", 3, 4, 5)]
        [InlineData("length(Name) gt 5", 3, 4, 5)]
        [InlineData("indexof(Name, 'eorge') eq 1", 3, 4, 5)]
        [InlineData("contains(Name, 'eorge')", 3, 4, 5)]
        [InlineData("replace(Name, 'George', 'ReplacedName') eq 'ReplacedName'", 3, 4, 5)]
        [InlineData("tolower(Name) eq 'george'", 3, 4, 5)]
        [InlineData("toupper(Name) eq 'GEORGE'", 3, 4, 5)]
        [InlineData("concat('Georg', 'e') eq Name", 3, 4, 5)]
        [InlineData("trim(' George ') eq Name", 3, 4, 5)]
        [InlineData("substring(Name, 1) eq 'eorge'", 3, 4, 5)]
        [InlineData("substring(Name, 0, 1) eq 'G'", 3, 4, 5)]
        public void Filter_Expression(string expression, params int[] expectedIds)
        {
            var queryable = SampleCollection.ToList().AsQueryable();
            var ft = new NoData.NoDataBuilder<Dto>("partner/partner", expression, null);
            var result = ft.Load(queryable).BuildQueryable();

            var ids = result.SelectMany(r => r.GetAllIds()).ToList();

            Assert.Equal(expectedIds.Length, ids.Count);
            foreach (var resultId in ids)
                Assert.Contains(resultId, expectedIds);
        }

        // More tests
        // substringof(Name,'eorg') // right is contained within the left paramter
        // substring(Name, 'Name', 'ReplacedName') eq 'ReplacedName'
    }
}
