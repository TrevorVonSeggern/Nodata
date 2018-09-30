using System.Collections.Generic;

namespace NoData.Tests.SharedExampleClasses
{
    public class Dto
    {
        public int id { get; set; }
        public string Name { get; set; }
        public string region_code { get; set; }
        public Dto partner { get; set; }
        public ICollection<DtoChild> children { get; set; } = new List<DtoChild>();
        public DtoChild favorite { get; set; }

        public IEnumerable<int> GetAllIds()
        {
            yield return id;
            // partner
            if (partner != null)
                foreach (var i in partner.GetAllIds())
                    yield return i;
            // favorite
            if (favorite != null)
                foreach (var i in favorite.GetAllIds())
                    yield return i;
            // children
            if (children != null)
                foreach (var child in children)
                    if (child != null)
                        foreach (var i in child.GetAllIds())
                            yield return i;
        }
    }
}