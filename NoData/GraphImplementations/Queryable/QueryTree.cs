using System.Collections.Generic;
using System.IO;
using System.Linq;
using QuickCache;
using Immutability;
using Graph;
using Graph.Interfaces;
using Newtonsoft.Json;
using NoData.GraphImplementations.Schema;
using Property = NoData.GraphImplementations.Schema.Property;

namespace NoData.GraphImplementations.Queryable
{
    [Immutable]
    public class QueryTree : Graph.Tree<QueryTree, QueryVertex, QueryEdge, QueryClass, Property>
    {
        public QueryTree(QueryVertex root, IEnumerable<ITuple<QueryEdge, QueryTree>> children = null) : base(root, children)
        {
        }

        public QueryTree(IEnumerable<IEnumerable<QueryEdge>> expandPaths) : base(expandPaths, ePaths => new QueryTree(ePaths), v => new QueryTree(v))
        {
        }

        // Serialization code
        public string AsJson<T>(T item) => this.ObjectAsJson(item);

        private static ICacheForever<int, FastMember.TypeAccessor> AccessorCache = new DictionaryCache<int, FastMember.TypeAccessor>();
        private string ObjectAsJson(object item)
        {
            if (item is null)
                return "null";
            var type = item.GetType();
            var key = type.GetHashCode();
            var accessor = AccessorCache.GetOrAdd(key, () => FastMember.TypeAccessor.Create(type));

            using (var sw = new StringWriter())
            using (var writer = new JsonTextWriter(sw))
            {
                writer.WriteStartObject(); // {

                foreach (var property in Root.Value.Properties.Where(p => p.IsPrimitive))
                {
                    writer.WritePropertyName(property.Name);
                    writer.WriteValue(accessor[item, property.Name]);
                }

                foreach (var property in Root.Value.Properties.Where(p => p.IsNavigationProperty))
                {
                    var childTree = Children.First(c => c.Item1.Value == property).Item2;
                    writer.WritePropertyName(property.Name);
                    var value = accessor[item, property.Name];
                    writer.WriteRawValue(childTree.ObjectAsJson(value)); // { child object }
                }

                foreach (var property in Root.Value.Properties.Where(p => p.IsCollection))
                {
                    var childTree = Children.First(c => c.Item1.Value == property).Item2;
                    var childList = (IEnumerable<object>)accessor[item, property.Name];
                    writer.WritePropertyName(property.Name);
                    writer.WriteStartArray();
                    foreach (var child in childList)
                    {
                        writer.WriteRawValue(childTree.ObjectAsJson(child));
                    }
                    writer.WriteEndArray();
                }

                writer.WriteEndObject(); // }

                return sw.ToString();
            }
        }
    }
}