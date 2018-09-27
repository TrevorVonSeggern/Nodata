using System.Collections.Generic;
using System.Linq;
using Graph;
using NoData.GraphImplementations;
using NoData.GraphImplementations.Queryable;
// Sorry for the confusing names between the three types of graph type classes.

namespace NoData.Utility
{
    public static class SchemaToQueryable
    {
        private static QueryPath CreateQueryPathFromSchemaPath(GraphImplementations.Schema.Path schemaPath, HashSet<QueryEdge> queryEdgeSet, IEnumerable<QueryVertex> queryVertices)
        {
            QueryEdge CreateEdge(GraphImplementations.Schema.Edge schemaEdge)
            {
                var queryEdge = queryEdgeSet.FirstOrDefault(qEdge => qEdge.Value.Equals(schemaEdge.Value));
                if (queryEdge is null)
                {
                    queryEdge = new QueryEdge(
                        queryVertices.First(v => v.Value.TypeId == schemaEdge.From.Value.TypeId),
                        queryVertices.First(v => v.Value.TypeId == schemaEdge.To.Value.TypeId),
                        schemaEdge.Value);
                }
                return queryEdge;
            }
            return new QueryPath(schemaPath.Select(sEdge => CreateEdge(sEdge)));
        }

        public static QueryTree Test(GraphImplementations.Schema.Tree expandTree, IEnumerable<GraphImplementations.Schema.PathToProperty> selections)
        {
            var schemaExpandPaths = expandTree.EnumerateAllPaths(x => new GraphImplementations.Schema.Path(x)).ToList();
            var filteredPaths = selections.Where(sel => schemaExpandPaths.Contains(sel));
            var propertySelectionList = filteredPaths.Select(f => ITuple.Create(f.Last().To.Value.TypeId, f.Property))
                .GroupBy(tuple => tuple.Item1) // group by schema vertex type id.
                .Select(x => ITuple.Create(x.Key, new List<GraphImplementations.Schema.Property>(x.Select(tup => tup.Item2)))) // get list of tuple <vertex, property>
                .ToList();

            // could throw an error here or something if there are select paths outside of the expand tree paths.

            var expandSubGraph = expandTree.Flatten();

            IEnumerable<GraphImplementations.Schema.Property> GetSelectionListForQueryClass(int typeId)
            {
                // property list for the vertex(of the same type(id))
                var pList = propertySelectionList.FirstOrDefault(p => p.Item1 == typeId);

                // if it's empty, return a list of normal properties.
                if (pList is null || !pList.Item2.Any())
                    return expandSubGraph.Vertices.First(x => x.Value.TypeId == typeId).Value.Properties;

                return pList.Item2; // return list from the select expression.
            }

            // get a list of properties from filtered paths/what should be serialized.
            var queryVertices = expandSubGraph.Vertices.Select(v => new QueryVertex(new QueryClass(v.Value.TypeId, GetSelectionListForQueryClass(v.Value.TypeId))));

            var queryEdgeSet = new HashSet<QueryEdge>();

            // construct a list of paths of the query class variety
            var queryPaths = schemaExpandPaths.Select(sEP => CreateQueryPathFromSchemaPath(sEP, queryEdgeSet, queryVertices));

            return new QueryTree(queryPaths);
        }
    }
}