using System.Collections.Generic;

namespace NoData.Tests.SharedExampleClasses.Database.Entity
{
    public class Person
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Region_code { get; set; }
        public int? PartnerId { get; set; }
        public int? FavoriteId { get; set; }

        public virtual Person Partner { get; set; }
        public virtual ICollection<Child> Children { get; set; }
        public virtual Child Favorite { get; set; }
    }
}