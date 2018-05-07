using NoData.Utility;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;

namespace BinaryExpressionParserTests
{
    [TestFixture]
    public class UtilityTest
    {
        public class Dto
        {
            public int id { get; set; }
            public string Name { get; set; }
            public string region_code { get; set; }
            public Dto partner { get; set; }
            public ICollection<DtoChild> children { get; set; }
        }

        public class DtoChild
        {
            public int id { get; set; }
            public string Name { get; set; }
            public string region_code { get; set; }
            public DtoChild partner { get; set; }
            public ICollection<DtoGrandChild> children { get; set; }
        }

        public class DtoGrandChild
        {
            public int id { get; set; }
            public string Name { get; set; }
            public string region_code { get; set; }
        }

        [Test]
        public void Utility_GetProperties()
        {
            var properties = ClassPropertiesUtility<Dto>.GetPropertyNames;
            var expected = new string[] { "id", "Name", "region_code", "partner", "children" };
            Assert.NotNull(properties);
            Assert.AreEqual(expected.Count(), properties.Count());
            foreach(var prop in properties)
            {
                Assert.Contains(prop, expected);
            }
        }

        [Test]
        public void Utility_GetExpandProperties()
        {
            var properties = ClassPropertiesUtility<Dto>.GetExpandablePropertyNames.ToList();

            var expected = new string[] { "partner", "children" };
            Assert.NotNull(properties);
            foreach(var prop in expected)
            {
                Assert.Contains(prop, properties);
            }
            Assert.AreEqual(expected.Count(), properties.Count());
        }

        [Test]
        public void Utility_GetNonExpandProperties()
        {
            var properties = ClassPropertiesUtility<Dto>.GetNonExpandablePropertyNames.ToList();

            var expected = new string[] { "id", "Name", "region_code" };
            Assert.NotNull(properties);
            foreach(var prop in expected)
            {
                Assert.Contains(prop, properties);
            }
            Assert.AreEqual(expected.Count(), properties.Count());
        }
    }
}
