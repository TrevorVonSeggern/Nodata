using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using NoData.Tests.SharedExampleClasses;

namespace NoData.Tests.FilterTests
{

    [TestFixture]
    public class FilterTest
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

        [TestCase("true", 1, 1, 2, 3, 4, 5, 6, 10)] // id comared to numbers.
        [TestCase("id eq 2", 2)] // id comared to numbers.
        [TestCase("id gt 2", 3, 4, 5, 6)]
        [TestCase("id ge 2", 2, 3, 4, 5, 6)]
        [TestCase("id lt 2", 1, 1, 10)]
        [TestCase("id le 2", 1, 1, 2, 10)]
        [TestCase("id ne 2", 1, 1, 3, 4, 5, 6, 10)]
        [TestCase("Name eq 'George'", 3, 4, 5)] // Name compared to strings.
        [TestCase("Name ne 'George'", 1, 1, 2, 6, 10)]
        [TestCase("id gt 3 and Name eq 'George'", 4, 5)] // conjunctions with logic.
        [TestCase("region_code ne 'en' and Name eq 'George'", 3, 5)]
        [TestCase("region_code eq 'de' and Name eq 'George'", 3)]
        [TestCase("(region_code eq 'de' or region_code eq 'es')and Name eq 'George'", 3, 5)] // space doesn't matter(tight).
        [TestCase("   ( region_code eq 'de' or region_code eq 'es'    ) and Name eq 'George'", 3, 5)] // space doesn't matter  (  loose  )   .
        [TestCase("Name eq 'George' or (region_code eq 'en' and id eq 4 ) ", 3, 4, 5)] // grouping order matters.
        [TestCase("(Name eq 'George' or region_code eq 'en') and id eq 4", 4)]
        [TestCase("id le 1 or id ge 1", 1, 1, 2, 3, 4, 5, 6, 10)] // multiple of the same property.
        [TestCase("id le 1 and id ge 1", 1, 1, 10)]
        [TestCase("id eq 1 or id eq 1 and id eq 1 or id eq 1 and id eq 1 or id eq 1", 1, 1, 10)] // duplication doesn't matter.
        [TestCase("partner/id eq 10", 1, 1, 10)] // duplication doesn't matter.
        [TestCase("partner/partner/id eq 1", 1, 1, 10)] // duplication doesn't matter.
        public void Filter_Expression(string expression, params int[] expectedIds)
        {
            var ft = new NoData.NoDataQuery<Dto>("partner/partner", expression, null);
            var result = ft.ApplyQueryable(new List<Dto>(SampleCollection).AsQueryable());

            var ids = result.SelectMany(r => r.GetAllIds()).ToList();

            Assert.AreEqual(expectedIds.Length, ids.Count);
            foreach (var resultId in ids)
                Assert.True(expectedIds.Contains(resultId));
        }

        // More tests
        // endswith(Name,'eorge')
        // startswith(Name,'george')
        // substringof(Name,'eorg') // right is contained within the left paramter
        // length(Name) gt 1
        // indexof(Name, 'ame') eq 1
        // replace(Name, 'Name', 'ReplacedName') eq 'ReplacedName'
        // substring(Name, 'Name', 'ReplacedName') eq 'ReplacedName'
    }
}
