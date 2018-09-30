using Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using NoData.GraphImplementations.QueryParser;

using QueueItem = NoData.GraphImplementations.QueryParser.Tree;
using TInfo = NoData.GraphImplementations.QueryParser.TextInfo;
using TGrouping = Graph.ITuple<string, System.Func<System.Collections.Generic.IList<NoData.GraphImplementations.QueryParser.Tree>, Graph.ITuple<NoData.GraphImplementations.QueryParser.Tree, int>>>;

namespace NoData.QueryParser.ParsingTools.Groupings
{
    public static class OrderGroupings
    {
        private static TGrouping Create(string pattern, Func<IList<QueueItem>, ITuple<QueueItem, int>> addFunc) => ITuple.Create(pattern, addFunc);

        public static TGrouping SortOrderProperty =
            Create($"{TInfo.ExpandProperty}({TInfo.SortOrder})?", list =>
            {
                var root = new Vertex(TInfo.FromRepresentation(TInfo.SortProperty));

                var expandProperty = list[0];
                var edgeExpand = new Edge(root, expandProperty.Root);

                var sortDirection = list.Count == 2 ?
                                        list[1] :
                                        new QueueItem(new Vertex(
                                            new TextInfo("asc", "asc", TextInfo.SortOrder)));
                var edgeSort = new Edge(root, sortDirection.Root);

                return ITuple.Create(new QueueItem(root, new[] {
                    ITuple.Create(edgeExpand, expandProperty),
                    ITuple.Create(edgeSort, sortDirection),
                }), list.Count - 1);
            });

        public static IEnumerable<TGrouping> ListOfSorting = new TGrouping[] {
            Create($"{TInfo.SortProperty}({TInfo.Comma}{TInfo.SortProperty})*", list =>
            {
                var filtered = list.Where(x => x.Representation == TInfo.SortProperty).ToList();
                var children = new Queue<QueueItem>(filtered);

                var root = new Vertex(TInfo.FromRepresentation(TInfo.ListOfSortings));
                var edges = children.Select(t => new Edge(root, t.Root));
                var childrenItems = new List<ITuple<Edge, QueueItem>>();
                foreach (var child in children)
                    childrenItems.Add(ITuple.Create(edges.First(e => e.To == child.Root), child));

                return ITuple.Create(new QueueItem(root, childrenItems), list.Count - 1);
            })
        };
    }
}
