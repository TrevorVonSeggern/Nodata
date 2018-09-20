using NoData.Utility;
using Xunit;
using System.Collections.Generic;
using System.Linq;
using NoData.Tests.SharedExampleClasses;

namespace NoData.Tests.UtilityTests
{
    public class ClassUtilityTest
    {
        [Fact]
        public void Utility_GetProperties()
        {
            var properties = ClassPropertiesUtility<Dto>.GetPropertyNames;
            var expected = new string[] { "id", "Name", "region_code", "partner", "children", "favorite" };
            Assert.NotNull(properties);
            Assert.Equal(expected.Length, properties.Count());
            foreach (var prop in properties)
            {
                Assert.Contains(prop, expected);
            }
        }

        [Fact]
        public void Utility_GetExpandProperties()
        {
            var properties = ClassPropertiesUtility<Dto>.GetExpandablePropertyNames.ToList();

            var expected = new string[] { "partner", "children", "favorite" };
            Assert.NotNull(properties);
            foreach (var prop in expected)
            {
                Assert.Contains(prop, properties);
            }
            Assert.Equal(expected.Length, properties.Count);
        }

        [Fact]
        public void Utility_GetNonExpandProperties()
        {
            var properties = ClassPropertiesUtility<Dto>.GetNonExpandablePropertyNames.ToList();

            var expected = new string[] { "id", "Name", "region_code" };
            Assert.NotNull(properties);
            foreach (var prop in expected)
            {
                Assert.Contains(prop, properties);
            }
            Assert.Equal(expected.Length, properties.Count);
        }


        public class DtoWithNullId
        {
            public int? id { get; set; }
        }

        [Fact]
        public void Utility_GetNonExpandedProperties_WithNullableInt()
        {
            var properties = ClassPropertiesUtility<DtoWithNullId>.GetNonExpandablePropertyNames.ToList();
            Assert.NotNull(properties);
            Assert.Contains("id", properties);
            Assert.Single(properties);
        }
    }
}
