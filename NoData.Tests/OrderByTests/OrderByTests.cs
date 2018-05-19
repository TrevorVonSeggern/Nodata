using System.Linq;
using NoData.Tests.SharedExampleClasses;
using NUnit.Framework;

namespace NoData.Tests.OrderByTests
{
    [TestFixture]
    public class OrderByTests
    {
        [Test]
        public void OrderBy_Id_InOrder()
        {
            var dtoEnum = Enumerable.Range(0, 100).Select(x => new Dto { id = 100 - x, Name = (100 - x).ToString(), region_code = "en" });
            var ids = new NoData.NoDataQuery<Dto>(null, null, null, "id").ApplyQueryable(dtoEnum.AsQueryable()).Select(x => x.id).ToList();
            Assert.That(ids, Is.EqualTo(ids.OrderBy(x => x)));
        }

    }
}