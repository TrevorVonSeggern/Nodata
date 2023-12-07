using Immutability;
using Property = NoData.GraphImplementations.Schema.Property;

namespace NoData.GraphImplementations.Queryable
{
    [Immutable]
    public class QueryClass
    {
        public IReadOnlyList<Property> Properties { get; }
        public int TypeId { get; }


        public QueryClass(int typeId, IEnumerable<Property> properties)
        {
            TypeId = typeId;
            Properties = properties.ToList().AsReadOnly();
        }

    }
}
