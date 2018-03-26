﻿using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace NoData.Graph.Utility
{
    public static class GraphUtility
    {
        /// <summary>
        /// Given the path string and the graph, it will return the property info, or null if it doesn't exist/isnot a primitive.
        /// </summary>
        /// <param name="path">Path to the property. Example: children/favorite/partner/id - would return Int32 </param>
        /// <param name="graph"></param>
        /// <returns></returns>
        public static PropertyInfo GetPropertyFromPathString(string path, Type root, Graph graph)
        {
            var split = path.Split('/');
            var info = NoData.Utility.ClassInfoCache.GetOrAdd(root);
            var vertex = graph.VertexContainingType(root);
            for(int i = 0; i < split.Length; ++i)
            {
                if (vertex == null) return null;
                if (info.NonExpandablePropertyNames.Contains(split[0]))
                    return info.NonExpandableProperties.Single(x => x.Name == split[0]);
                vertex = graph.OutgoingEdges(vertex).FirstOrDefault(e => e.Value.PropertyName == split[i]).To;
                info = NoData.Utility.ClassInfoCache.GetOrAdd(vertex.Value.Type);
            }
            if (split.Length == 1)
            {
            }
            return null;
        }
    }
}
