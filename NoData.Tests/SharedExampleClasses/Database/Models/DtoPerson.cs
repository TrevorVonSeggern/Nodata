using System.Collections.Generic;

namespace NoData.Tests.SharedExampleClasses.Database.Models
{
    public class DtoPerson
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Region_code { get; set; }

        public int? PartnerId { get; set; }
        public int? FavoriteId { get; set; }

        public virtual DtoPerson Partner { get; set; }
        public virtual ICollection<DtoChild> Children { get; set; }
        public virtual DtoChild Favorite { get; set; }
    }
}