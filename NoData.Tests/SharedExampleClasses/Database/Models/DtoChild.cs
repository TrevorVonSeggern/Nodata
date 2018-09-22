using System.Collections.Generic;

namespace NoData.Tests.SharedExampleClasses.Database.Models
{
    public class DtoChild
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Region_code { get; set; }
        public int? PartnerId { get; set; }
        public int? FavoriteId { get; set; }
        public DtoChild Partner { get; set; }
        public ICollection<DtoGrandChild> Children { get; set; }
        public DtoGrandChild Favorite { get; set; }
    }
}