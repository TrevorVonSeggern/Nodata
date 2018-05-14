﻿using NoData.Graph.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using QueueItem = NoData.QueryParser.Graph.Tree;
using TInfo = NoData.QueryParser.Graph.TextInfo;
using TGrouping = NoData.Graph.Base.ITuple<string, System.Func<System.Collections.Generic.IList<NoData.QueryParser.Graph.Tree>, NoData.Graph.Base.ITuple<NoData.QueryParser.Graph.Tree, int>>>;

namespace NoData.QueryParser.ParsingTools.Groupings
{
    public static class ExpandGroupings
    {
        private static TGrouping Create(string pattern, Func<IList<QueueItem>, ITuple<QueueItem, int>> addFunc) => ITuple.Create(pattern, addFunc);

        public static TGrouping ExpandProperty = Create($"{TInfo.ClassProperty}({TInfo.ForwardSlash}{TInfo.ClassProperty})*", list =>
            {
                var itemList = new Stack<Graph.Vertex>();
                itemList.Push(list[0].Root);

                var toRemove = 0;
                while (toRemove + 2 < list.Count)
                {
                    var representation = list[toRemove + 1].Root.Value.Representation;
                    if (list[toRemove + 1].Root.Value.Representation == TInfo.ForwardSlash &&
                        list[toRemove + 2].Root.Value.Representation == TInfo.ClassProperty)
                    {
                        itemList.Push(list[toRemove + 2].Root);
                        toRemove += 2;
                    }
                    else break;
                }

                var vertex = itemList.Pop();
                vertex.Value.Representation = TInfo.ExpandProperty;
                QueueItem propertyTree = new QueueItem(vertex, vertex.Value.Text);

                // build from linear list, the tree
                for (vertex = itemList.Any() ? itemList.Peek() : null; itemList.Count() != 0; vertex = itemList.Pop())
                {
                    var edge = new Graph.Edge(vertex, propertyTree.Root);
                    vertex.Value.Representation = TInfo.ExpandProperty;
                    propertyTree = new QueueItem(vertex, new[] { ITuple.Create(edge, propertyTree) });
                }
                return ITuple.Create(propertyTree, toRemove);
            });

        public static IEnumerable<TGrouping> ListOfExpand = new TGrouping[] {
            Create(TInfo.ListOfExpands + TInfo.Comma + TInfo.ExpandProperty, list =>
            {
                var filtered = list.Where(x => x.Root.Value.Representation != TInfo.Comma).ToList();
                var grouped = filtered[0];
                var expand = filtered[1];
                var children = new List<ITuple<Graph.Edge, QueueItem>>(grouped.Children)
                {
                    ITuple.Create(new Graph.Edge(grouped.Root, expand.Root), expand)
                };
                var raw = new List<string>();

                grouped = new QueueItem(grouped.Root, children, raw);
                return ITuple.Create(grouped, 2);
            }),
            Create(TInfo.ExpandProperty + TInfo.Comma + TInfo.ListOfExpands, list =>
            {
                var filtered = list.Where(x => x.Root.Value.Representation != TInfo.Comma).ToList();
                var grouped = filtered[1];
                var expand = filtered[0];
                var children = new List<ITuple<Graph.Edge, QueueItem>>(grouped.Children)
                {
                    ITuple.Create(new Graph.Edge(grouped.Root, expand.Root), expand)
                };
                var raw = new List<string>();

                grouped = new QueueItem(grouped.Root, children, raw);
                return ITuple.Create(grouped, 2);
            }),
            Create(TInfo.ListOfExpands + TInfo.Comma + TInfo.ListOfExpands, list =>
            {
                var filtered = list.Where(x => x.Root.Value.Representation != TInfo.Comma).ToList();
                var grouped = filtered[0];
                var otherGrouped = filtered[1];
                var children = new List<ITuple<Graph.Edge, QueueItem>>(grouped.Children);
                foreach (var child in otherGrouped.Children)
                    children.Add(child);
                var raw = new List<string>();

                grouped = new QueueItem(grouped.Root, children, raw);
                return ITuple.Create(grouped, 2);
            }),
            Create($"{TInfo.ExpandProperty}({TInfo.Comma}{TInfo.ExpandProperty})*", list =>
            {
                var children = new Queue<QueueItem>();
                children.Enqueue(list[0]);

                var toRemove = 0;
                while (toRemove + 2 < list.Count)
                {
                    var representation = list[toRemove + 1].Root.Value.Representation;
                    if (list[toRemove + 1].Root.Value.Representation == TInfo.Comma &&
                        list[toRemove + 2].Root.Value.Representation == TInfo.ExpandProperty)
                    {
                        children.Enqueue(list[toRemove + 2]);
                        toRemove += 2;
                    }
                    else break;
                }

                var root = new Graph.Vertex(new TInfo { Representation = TInfo.ListOfExpands });
                var edges = children.Select(t => new Graph.Edge(root, t.Root));
                var childrenItems = new List<ITuple<Graph.Edge, QueueItem>>();
                foreach (var child in children)
                    childrenItems.Add(ITuple.Create(edges.First(e => e.To == child.Root), child));

                QueueItem item = new QueueItem(root, childrenItems);

                return ITuple.Create(item, toRemove);
            })
        };

        public static TGrouping ListOfClauseExpressions()
        {
            var expand = TInfo.ExpandExpression;
            var filter = TInfo.FilterExpression;
            var select = TInfo.SelectExpression;

            var clause = $"({expand}|{filter}|{select})";
            return Create($"{clause}({TInfo.SemiColin}{clause})*", list =>
            {
                var root = new Graph.Vertex(new TInfo { Representation = TInfo.ListOfClause, Type = typeof(TInfo) });

                bool IsClause(QueueItem item)
                {
                    if (item.Representation == expand) return true;
                    if (item.Representation == filter) return true;
                    if (item.Representation == select) return true;
                    return false;
                }

                var childrenWithEdge = list.Where(IsClause).Select(x => ITuple.Create(new Graph.Edge(root, x.Root), x));
                return ITuple.Create(new QueueItem(root, childrenWithEdge), list.Count - 1);
            });
        }

        public static TGrouping FilterExpression = Create(TInfo.FilterClause + TInfo.BooleanValue, list =>
            {
                var root = new Graph.Vertex(new TInfo { Representation = TInfo.FilterExpression });
                return ITuple.Create(new QueueItem(root, new[] { ITuple.Create(new Graph.Edge(root, list[1].Root), list[1]) }), list.Count() - 1);
            });

        public static TGrouping SelectExpression = Create(TInfo.SelectClause + TInfo.ListOfExpands, list =>
            {
                var root = new Graph.Vertex(new TInfo { Representation = TInfo.SelectExpression });
                return ITuple.Create(new QueueItem(root, new[] { ITuple.Create(new Graph.Edge(root, list[1].Root), list[1]) }), 1);
            });

        public static TGrouping ExpandExpression = Create(TInfo.ExpandClause + TInfo.ListOfExpands, list =>
            {
                var root = new Graph.Vertex(new TInfo { Representation = TInfo.ExpandExpression });
                return ITuple.Create(new QueueItem(root, new[] { ITuple.Create(new Graph.Edge(root, list[1].Root), list[1]) }), 1);
            });

        public static TGrouping ExpandPropertyWithListOfClauses = Create($"{TInfo.ExpandProperty}{TInfo.OpenParenthesis}{TInfo.ListOfClause}?{TInfo.CloseParenthesis}", list =>
            {
                // add one to the children
                var root = list[0].Root;
                var children = list[0].Children.ToList();
                children.Add(ITuple.Create(new Graph.Edge(root, list[1].Root), list[1]));
                return ITuple.Create(new QueueItem(root, children), 3);
            });

    }
}
