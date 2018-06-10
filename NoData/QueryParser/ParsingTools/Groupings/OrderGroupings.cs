using NoData.Graph.Base;
using System;
using System.Collections.Generic;
using QueueItem = NoData.QueryParser.Graph.Tree;
using TInfo = NoData.QueryParser.Graph.TextInfo;
using TGrouping = NoData.Graph.Base.ITuple<string, System.Func<System.Collections.Generic.IList<NoData.QueryParser.Graph.Tree>, NoData.Graph.Base.ITuple<NoData.QueryParser.Graph.Tree, int>>>;
using System.Linq;

namespace NoData.QueryParser.ParsingTools.Groupings
{
    public static class OrderGroupings
    {
        private static TGrouping Create(string pattern, Func<IList<QueueItem>, ITuple<QueueItem, int>> addFunc) => ITuple.Create(pattern, addFunc);

        public static TGrouping SortOrderProperty =
            Create(TInfo.ExpandProperty + TInfo.SortOrder, list =>
            {
                var root = new Graph.Vertex(new TInfo { Representation = TInfo.SortProperty });
                var expandProperty = list[0];
                var edgeExpand = new Graph.Edge(root, expandProperty.Root);
                var sortDirection = list[1];
                var edgeSort = new Graph.Edge(root, sortDirection.Root);
                return ITuple.Create(new QueueItem(root, new[] {
                    ITuple.Create(edgeExpand, new QueueItem(expandProperty.Root)),
                    ITuple.Create(edgeSort, new QueueItem(sortDirection.Root)),
                }), 1);
            });

        public static IEnumerable<TGrouping> ListOfSorting = new TGrouping[] {
            Create(TInfo.ListOfSortings + TInfo.Comma + TInfo.SortProperty, list =>
            {
                var filtered = list.Where(x => x.Root.Value.Representation != TInfo.Comma).ToList();
                var grouped = filtered[0];
                var expand = filtered[1];
                var children = new List<ITuple<Graph.Edge, QueueItem>>(grouped.Children)
                {
                    ITuple.Create(new Graph.Edge(grouped.Root, expand.Root), expand)
                };

                return ITuple.Create(new QueueItem(grouped.Root, children), 2);
            }),
            Create(TInfo.SortProperty + TInfo.Comma + TInfo.ListOfSortings, list =>
            {
                var filtered = list.Where(x => x.Root.Value.Representation != TInfo.Comma).ToList();
                var grouped = filtered[1];
                var expand = filtered[0];
                var children = new List<ITuple<Graph.Edge, QueueItem>>(grouped.Children)
                {
                    ITuple.Create(new Graph.Edge(grouped.Root, expand.Root), expand)
                };

                return ITuple.Create(new QueueItem(grouped.Root, children), 2);
            }),
            Create(TInfo.ListOfSortings + TInfo.Comma + TInfo.ListOfSortings, list =>
            {
                var filtered = list.Where(x => x.Root.Value.Representation != TInfo.Comma).ToList();
                var grouped = filtered[0];
                var otherGrouped = filtered[1];
                var children = new List<ITuple<Graph.Edge, QueueItem>>(grouped.Children);
                foreach (var child in otherGrouped.Children)
                    children.Add(child);
                var raw = new List<string>();

                return ITuple.Create(new QueueItem(grouped.Root, children), 2);
            }),
            Create($"{TInfo.SortProperty}({TInfo.Comma}{TInfo.SortProperty})*", list =>
            {
                var filtered = list.Where(x => x.Representation == TInfo.SortProperty).ToList();
                var children = new Queue<QueueItem>(filtered);

                var root = new Graph.Vertex(new TInfo { Representation = TInfo.ListOfSortings });
                var edges = children.Select(t => new Graph.Edge(root, t.Root));
                var childrenItems = new List<ITuple<Graph.Edge, QueueItem>>();
                foreach (var child in children)
                    childrenItems.Add(ITuple.Create(edges.First(e => e.To == child.Root), child));

                return ITuple.Create(new QueueItem(root, childrenItems), (list.Count - filtered.Count) - 1);
            })
        };
    }
}
