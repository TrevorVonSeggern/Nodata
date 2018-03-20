using System.Collections.Generic;

namespace nodata.Models
{
    public class Dto
    {
        public int id { get; set; }
        public string Name { get; set; }
        public string region_code { get; set; }
        public Dto partner { get; set; }
        public ICollection<DtoChild> children { get; set; }
        public DtoChild favorite { get; set; }
    }
    public class DtoChild
    {
        public int id { get; set; }
        public string Name { get; set; }
        public string region_code { get; set; }
        public DtoChild partner { get; set; }
        public ICollection<DtoGrandChild> children { get; set; }
        public DtoGrandChild favorite { get; set; }
    }

    public class DtoGrandChild
    {
        public int id { get; set; }
        public string Name { get; set; }
        public string region_code { get; set; }
    }

}