using System.Collections.Generic;

namespace SampleEFCoreApi.Models
{
    public class DtoPerson
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Region_code { get; set; }

        public int? PartnerId { get; set; }
        public int? FavoriteId { get; set; }

        public DtoPerson Partner { get; set; }
        public ICollection<DtoChild> Children { get; set; }
        public DtoChild Favorite { get; set; }
    }
}