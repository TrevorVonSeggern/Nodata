using System.Collections.Generic;

namespace SampleEFCoreApi.Models
{
    public class DtoChildModify
    {
        public string Name { get; set; }
        public string Region_code { get; set; }

        public int? PartnerId { get; set; }
        public int? FavoriteId { get; set; }
    }
}