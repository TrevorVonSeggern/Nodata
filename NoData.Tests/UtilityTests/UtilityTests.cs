using NoData.Utility;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using NoData.Tests.SharedExampleClasses;

namespace NoData.Tests.UtilityTests
{
    [TestFixture]
    public class UtilityTest
    {
        [Test]
        public void Utility_GetProperties()
        {
            var properties = ClassPropertiesUtility<Dto>.GetPropertyNames;
            var expected = new string[] { "id", "Name", "region_code", "partner", "children", "favorite" };
            Assert.NotNull(properties);
            Assert.AreEqual(expected.Count(), properties.Count());
            foreach (var prop in properties)
            {
                Assert.Contains(prop, expected);
            }
        }

        [Test]
        public void Utility_GetExpandProperties()
        {
            var properties = ClassPropertiesUtility<Dto>.GetExpandablePropertyNames.ToList();

            var expected = new string[] { "partner", "children", "favorite" };
            Assert.NotNull(properties);
            foreach (var prop in expected)
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
            foreach (var prop in expected)
            {
                Assert.Contains(prop, properties);
            }
            Assert.AreEqual(expected.Count(), properties.Count());
        }
    }
}
