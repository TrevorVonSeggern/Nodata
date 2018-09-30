using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

using NoData.GraphImplementations.Schema;

namespace NoData.Utility
{
    using Graph.Interfaces;

    public static class GraphExtension
    {
        /// <summary>
        /// Given the path string and the graph, it will return the property info, or null if it doesn't exist/is not a primitive.
        /// </summary>
        /// <param name="path">Path to the property. Example: children/favorite/partner/id - would return Int32 </param>
        /// <param name="graph"></param>
        /// <returns></returns>
        // public static PropertyInfo GetPropertyFromPathString(string path, Type root, GraphSchema graph)
        // {
        //     var split = path.Split('/');
        //     var info = graph.VertexContainingType(root).Value;
        //     // var info = Utility.ClassInfoCache.GetOrAdd(root);
        //     var vertex = graph.VertexContainingType(root);
        //     for (int i = 0; i < split.Length - 1; ++i)
        //     {
        //         if (vertex == null) return null;
        //         if (info.NonExpandablePropertyNames.Contains(split[0]))
        //             return info.NonExpandableProperties.Single(x => x.Name == split[0]);
        //         var v = graph.OutgoingEdges(vertex).FirstOrDefault(e => e.Value.PropertyName == split[i])?.To;
        //         if (v is null)
        //             break;
        //         vertex = v;
        //         info = Utility.ClassInfoCache.GetOrAdd(vertex.Value.Type);
        //     }

        //     return info.NonExpandableProperties.FirstOrDefault(p => p.Name == split[split.Length - 1]) ?? throw new ArgumentOutOfRangeException();
        // }
    }
}
