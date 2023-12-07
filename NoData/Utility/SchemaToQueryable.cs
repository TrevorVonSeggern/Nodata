using Graph;
using NoData.GraphImplementations.Queryable;
// Sorry for the confusing names between the three types of graph type classes.

using Property = NoData.GraphImplementations.Schema.Property;
using PathToProperty = NoData.GraphImplementations.Schema.PathToProperty;

namespace NoData.Utility
{
    public static class SchemaToQueryable
    {
        private static QueryEdge CreateEdge(GraphImplementations.Schema.Edge schemaEdge, IEnumerable<QueryVertex> queryVertices)
        {
            var queryEdge = new QueryEdge(
                queryVertices.First(v => v.Value.TypeId == schemaEdge.From.Value.TypeId),
                queryVertices.First(v => v.Value.TypeId == schemaEdge.To.Value.TypeId),
                schemaEdge.Value);
            return queryEdge;
        }
        private static QueryPath CreateQueryPathFromSchemaPath(GraphImplementations.Schema.Path schemaPath, IEnumerable<QueryVertex> queryVertices)
        {
            return new QueryPath(schemaPath.Select(sEdge => CreateEdge(sEdge, queryVertices)));
        }

        public static QueryTree TranslateTree(GraphImplementations.Schema.Tree expandTree, IEnumerable<GraphImplementations.Schema.PathToProperty> selections)
        {
            var schemaExpandPaths = expandTree.EnumerateAllPaths(x => new GraphImplementations.Schema.Path(x)).ToList();
            var filteredPaths = selections.Where(sel => schemaExpandPaths.Contains(sel));
            var typeIdByPropertySelected = new[]{
                    filteredPaths.Select(f => ITuple.Create(f.Last().From.Value.TypeId, f.Property)),
                    filteredPaths.Select(f => ITuple.Create(f.Last().To.Value.TypeId, f.Property)),
                }.SelectMany(x => x)
                .GroupBy(tuple => tuple.Item1) // group by schema vertex type id.
                .Select(x => ITuple.Create(x.Key, new List<Property>(x.Select(tup => tup.Item2).Distinct()))) // get list of tuple <vertex, property>
                .ToList();
            var typeIdByNavigationSelected = new[]{
                    schemaExpandPaths.Select(f => ITuple.Create<int, Property>(f.Last().From.Value.TypeId, f.Last().Value)),
                    schemaExpandPaths.Select(f => ITuple.Create<int, Property>(f.Last().To.Value.TypeId, f.Last().Value))
                }.SelectMany(x => x)
                .GroupBy(tuple => tuple.Item1) // group by schema vertex type id.
                .Select(x => ITuple.Create(x.Key, new List<Property>(x.Select(tup => tup.Item2).Distinct()))) // get list of tuple <vertex, property>
                .ToList();


            // could throw an error here or something if there are select paths outside of the expand tree paths.

            var expandSubGraph = expandTree.Flatten();

            IEnumerable<Property> GetSelectionListForQueryClass(int typeId)
            {
                // property list for the vertex(of the same type(id))
                var pList = typeIdByPropertySelected.FirstOrDefault(p => p.Item1 == typeId);
                var result = new List<Property>();
                // if it's empty, return a list of normal properties.
                if (pList is null || !pList.Item2.Any())
                    result.AddRange(expandSubGraph.Vertices.First(x => x.Value.TypeId == typeId).Value.Properties.Where(p => p.IsPrimitive));
                else
                    result.AddRange(pList.Item2);

                var navList = typeIdByNavigationSelected.FirstOrDefault(p => p.Item1 == typeId)?.Item2;
                if (navList != null && navList.Any())
                    result.AddRange(navList);

                return result; // return list from the select expression.
            }

            // get a list of properties from filtered paths/what should be serialized.
            var queryVertices = expandSubGraph.Vertices.Select(v => new QueryVertex(new QueryClass(v.Value.TypeId, GetSelectionListForQueryClass(v.Value.TypeId))));

            // construct a list of paths of the query class variety
            var queryPaths = schemaExpandPaths.Select(sEP => CreateQueryPathFromSchemaPath(sEP, queryVertices));

            // if nothing is selected, we still need to return the root.
            if (queryPaths.Any())
                return new QueryTree(queryPaths);
            return new QueryTree(queryVertices.FirstOrDefault());
        }

        private static IEnumerable<Property> DefaultProperties(GraphImplementations.Schema.Vertex vertex)
        {
            return vertex.Value.Properties.Where(property => property.IsPrimitive);
        }


        public static QueryTree TranslateTree2(GraphImplementations.Schema.Tree expandTree, IEnumerable<PathToProperty> selections)
        {
            var schemaExpandPaths = expandTree.EnumerateAllPaths(x => new GraphImplementations.Schema.Path(x)).ToList();

            return CreateQueryTree(expandTree.Root, schemaExpandPaths, selections);
        }

        public static QueryTree CreateQueryTree(
            GraphImplementations.Schema.Vertex root,
            IEnumerable<IEnumerable<GraphImplementations.Schema.Edge>> expandPaths,
            IEnumerable<PathToProperty> selections)
        {
            var propertyList = new List<Property>();
            // add selections to serialize list
            var selectedProperties = selections.Where(s => s.Count() == 0).Select(x => x.Property).ToList();
            if (selectedProperties.Any())
                propertyList.AddRange(selectedProperties);
            else
                propertyList.AddRange(DefaultProperties(root));

            if (!expandPaths.Any())
                return new QueryTree(new QueryVertex(new QueryClass(root.Value.TypeId, propertyList)));

            var childList = new List<ITuple<QueryEdge, QueryTree>>();

            // serialize each edge.
            propertyList.AddRange(expandPaths.GroupBy(x => x.First()).Select(x => x.Key.Value));

            var queryRoot = new QueryVertex(new QueryClass(root.Value.TypeId, propertyList));

            // each path that has the the same root.
            foreach (var path in expandPaths.GroupBy(x => x.First()))
            {
                var childPaths = path.Select(p => p.Skip(1)).Where(p => p.Any());
                var childSelects = selections.Where(x => x.Any()).Select(p => new PathToProperty(p.Skip(1), p.Property));

                var childTree = CreateQueryTree(path.Key.To, childPaths, childSelects);
                var edge = new QueryEdge(queryRoot, childTree.Root, path.Key.Value);

                childList.Add(ITuple.Create(edge, childTree));
            }

            return new QueryTree(queryRoot, childList);
        }
    }
}
