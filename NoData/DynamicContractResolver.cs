using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using NoData.GraphImplementations.Schema;

namespace NoData
{
    public class DynamicContractResolver : DefaultContractResolver
    {
        public readonly GraphSchema Graph;

        public DynamicContractResolver(GraphSchema graph)
        {
            Graph = graph;
        }

        protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
        {
            IList<JsonProperty> retval = base.CreateProperties(type, memberSerialization);

            retval = retval.ToList();

            var result = new List<JsonProperty>();
            foreach (var property in retval)
            {
                property.ShouldSerialize = instance =>
                {
                    if (property.Ignored)
                        return false;

                    var vertex = Graph.Vertices.FirstOrDefault(v => v.Value.TypeId == instance.GetType().GetHashCode());
                    if (vertex is null) return false;
                    // return vertex.Value.ShouldSerializeProperty(instance, property.PropertyName);
                    return true;
                };
                result.Add(property);
            }

            return result;
        }
    }
}
