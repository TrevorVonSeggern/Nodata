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
            var dtoEnum = Enumerable.Range(0, 100).Select(x => new Dto { id = 100 - x, Name = (-1).ToString(), region_code = "en" });
            var ids = new NoData.NoDataQuery<Dto>(null, null, null, nameof(Dto.id)).ApplyQueryable(dtoEnum.AsQueryable()).Select(x => x.id).ToList();
            Assert.That(ids, Is.EqualTo(ids.OrderBy(x => x)));
        }

        [Test]
        public void OrderBy_Name_InOrder()
        {
            var dtoEnum = Enumerable.Range(0, 100).Select(x => new Dto { id = -1, Name = (100 - x).ToString(), region_code = "en" });
            var names = new NoData.NoDataQuery<Dto>(null, null, null, nameof(Dto.Name)).ApplyQueryable(dtoEnum.AsQueryable()).Select(x => x.Name).ToList();
            Assert.AreEqual(names, names.OrderBy(x => x));
        }

        [Test]
        public void OrderBy_Id_InOrder_Asc()
        {
            var dtoEnum = Enumerable.Range(0, 100).Select(x => new Dto { id = 100 - x, Name = (-1).ToString(), region_code = "en" });
            var ids = new NoData.NoDataQuery<Dto>(null, null, null, $"{nameof(Dto.id)} asc").ApplyQueryable(dtoEnum.AsQueryable()).Select(x => x.id).ToList();
            Assert.That(ids, Is.EqualTo(ids.OrderBy(x => x)));
        }

        [Test]
        public void OrderBy_Name_InOrder_Asc()
        {
            var dtoEnum = Enumerable.Range(0, 100).Select(x => new Dto { id = -1, Name = (100 - x).ToString(), region_code = "en" });
            var names = new NoData.NoDataQuery<Dto>(null, null, null, $"{nameof(Dto.Name)} asc").ApplyQueryable(dtoEnum.AsQueryable()).Select(x => x.Name).ToList();
            Assert.AreEqual(names, names.OrderBy(x => x));
        }

        [Test]
        public void OrderBy_Id_InOrder_Desc()
        {
            var dtoEnum = Enumerable.Range(0, 100).Select(x => new Dto { id = 100 - x, Name = (-1).ToString(), region_code = "en" });
            var ids = new NoData.NoDataQuery<Dto>(null, null, null, $"{nameof(Dto.id)} desc").ApplyQueryable(dtoEnum.AsQueryable()).Select(x => x.id).ToList();
            Assert.That(ids, Is.EqualTo(ids.OrderByDescending(x => x)));
        }

        [Test]
        public void OrderBy_Name_InOrder_Desc()
        {
            var dtoEnum = Enumerable.Range(0, 100).Select(x => new Dto { id = -1, Name = (100 - x).ToString(), region_code = "en" });
            var names = new NoData.NoDataQuery<Dto>(null, null, null, $"{nameof(Dto.Name)} desc").ApplyQueryable(dtoEnum.AsQueryable()).Select(x => x.Name).ToList();
            Assert.AreEqual(names, names.OrderByDescending(x => x));
        }
    }
}