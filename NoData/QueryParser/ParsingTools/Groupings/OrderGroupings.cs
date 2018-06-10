using NoData.Graph.Base;
using System;
using System.Collections.Generic;
using QueueItem = NoData.QueryParser.Graph.Tree;
using TInfo = NoData.QueryParser.Graph.TextInfo;
using TGrouping = NoData.Graph.Base.ITuple<string, System.Func<System.Collections.Generic.IList<NoData.QueryParser.Graph.Tree>, NoData.Graph.Base.ITuple<NoData.QueryParser.Graph.Tree, int>>>;
using System.Linq;
using NoData.QueryParser.Graph;

namespace NoData.QueryParser.ParsingTools.Groupings
{
    public static class OrderGroupings
    {
        private static TGrouping Create(string pattern, Func<IList<QueueItem>, ITuple<QueueItem, int>> addFunc) => ITuple.Create(pattern, addFunc);

        public static TGrouping SortOrderProperty =
            Create($"{TInfo.ExpandProperty}({TInfo.SortOrder})?", list =>
            {
                var root = new Graph.Vertex(new TInfo { Representation = TInfo.SortProperty });

                var expandProperty = list[0];
                var edgeExpand = new Graph.Edge(root, expandProperty.Root);

                var sortDirection = list.Count == 2 ?
                                        list[1] :
                                        new QueueItem(new NoData.QueryParser.Graph.Vertex(
                                            new TextInfo()
                                            {
                                                Representation = TextInfo.SortOrder,
                                                Value = "asc",
                                                Text = "asc"
                                            }));
                var edgeSort = new Graph.Edge(root, sortDirection.Root);

                return ITuple.Create(new QueueItem(root, new[] {
                    ITuple.Create(edgeExpand, new QueueItem(expandProperty.Root)),
                    ITuple.Create(edgeSort, new QueueItem(sortDirection.Root)),
                }), list.Count - 1);
            });

        public static IEnumerable<TGrouping> ListOfSorting = new TGrouping[] {
            Create($"{TInfo.SortProperty}({TInfo.Comma}{TInfo.SortProperty})*", list =>
            {
                var filtered = list.Where(x => x.Representation == TInfo.SortProperty).ToList();
                var children = new Queue<QueueItem>(filtered);

                var root = new Graph.Vertex(new TInfo { Representation = TInfo.ListOfSortings });
                var edges = children.Select(t => new Graph.Edge(root, t.Root));
                var childrenItems = new List<ITuple<Graph.Edge, QueueItem>>();
                foreach (var child in children)
                    childrenItems.Add(ITuple.Create(edges.First(e => e.To == child.Root), child));

                return ITuple.Create(new QueueItem(root, childrenItems), list.Count - 1);
            })
        };
    }
}
