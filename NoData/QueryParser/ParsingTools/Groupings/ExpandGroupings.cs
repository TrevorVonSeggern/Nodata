﻿using NoData.Graph.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using QueueItem = NoData.QueryParser.Graph.Tree;
using TInfo = NoData.QueryParser.Graph.TextInfo;
using TGrouping = NoData.Graph.Base.ITuple<string, System.Func<System.Collections.Generic.IList<NoData.QueryParser.Graph.Tree>, NoData.Graph.Base.ITuple<NoData.QueryParser.Graph.Tree, int>>>;

namespace NoData.QueryParser.ParsingTools.Groupings
{
    static internal class ExpandGroupings
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

                // build from linear list, the tree
                QueueItem item = null;
                var raw = new List<string>();
                while (itemList.Count() != 0)
                {
                    var temp = itemList.Pop();
                    temp.Value.Representation = TInfo.ExpandProperty;
                    if (item == null)
                        item = new QueueItem(temp, temp.Value.Text);
                    else
                    {
                        var edge = new Graph.Edge(temp, item.Root);
                        raw.Add(temp.Value.Text);
                        item = new QueueItem(temp, new[] { ITuple.Create(edge, item) }, raw);
                    }
                }

                return ITuple.Create(item, toRemove);
            });

        public static IEnumerable<TGrouping> CollectionOfExpandProperty = new TGrouping[] {
            Create(TInfo.ListOfExpands + TInfo.Comma + TInfo.ExpandProperty, list =>
            {
                var grouped = list[0];
                var expand = list[2];
                var children = new List<ITuple<Graph.Edge, QueueItem>>(grouped.Children)
                {
                    ITuple.Create(new Graph.Edge(grouped.Root, expand.Root), expand)
                };
                var raw = new List<string>();
                raw.AddRange(grouped.RawText);
                raw.Add(",");
                raw.AddRange(expand.RawText);

                grouped = new QueueItem(grouped.Root, children, raw);
                return ITuple.Create(grouped, 2);
            }),
            Create(TInfo.ExpandProperty + TInfo.Comma + TInfo.ListOfExpands, list =>
            {
                var grouped = list[2];
                var expand = list[0];
                var children = new List<ITuple<Graph.Edge, QueueItem>>(grouped.Children)
                {
                    ITuple.Create(new Graph.Edge(grouped.Root, expand.Root), expand)
                };
                var raw = new List<string>();
                raw.AddRange(expand.RawText);
                raw.Add(",");
                raw.AddRange(grouped.RawText);

                grouped = new QueueItem(grouped.Root, children, raw);
                return ITuple.Create(grouped, 2);
            }),
            Create(TInfo.ListOfExpands + TInfo.Comma + TInfo.ListOfExpands, list =>
            {
                var grouped = list[0];
                var otherGrouped = list[2];
                var children = new List<ITuple<Graph.Edge, QueueItem>>(grouped.Children);
                foreach (var child in otherGrouped.Children)
                    children.Add(child);
                var raw = new List<string>();
                raw.AddRange(grouped.RawText);
                raw.Add(",");
                raw.AddRange(otherGrouped.RawText);

                grouped = new QueueItem(grouped.Root, children, raw);
                return ITuple.Create(grouped, 2);
            }),
            Create($"{TInfo.ExpandProperty}(\\/{TInfo.ExpandProperty})*", list =>
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
                var raw = new List<string>();
                foreach (var child in children)
                {
                    childrenItems.Add(ITuple.Create(edges.First(e => e.To == child.Root), child));
                    raw.AddRange(child.RawText);
                }

                QueueItem item = new QueueItem(root, childrenItems, raw);

                return ITuple.Create(item, toRemove);
            })
        };

        public static TGrouping SelectClauseIntegration<TRoot>(NoData.Graph.Graph graph, IAcceptAdditions selectParser)
        {
            return Create(TInfo.ListOfExpands + TInfo.OpenParenthesis + TInfo.SelectClause + TInfo.ListOfExpands + TInfo.CloseParenthesis, list =>
            {
                var edges = ExpandClaseParser<TRoot>._ExpandPropertyToEdgeList(list[0].Children.Last(), graph);

                string rootPath = string.Join("/", edges.Select(x => x.Value.PropertyName));

                var expandPaths = new List<string>();
                foreach(var expand in list[3].Children)
                {
                    expandPaths.Add(rootPath + "/" + string.Join("/", expand.Item2.RawText));
                }

                selectParser.AddToClause(string.Join(",", expandPaths));
                return ITuple.Create(list[0], 4);
            });
        }
    }
}