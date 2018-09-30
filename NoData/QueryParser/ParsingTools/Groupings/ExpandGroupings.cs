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
    public static class ExpandGroupings
    {
        private static TGrouping Create(string pattern, Func<IList<QueueItem>, ITuple<QueueItem, int>> addFunc) => ITuple.Create(pattern, addFunc);

        public static TGrouping ExpandProperty = Create($"{TInfo.ClassProperty}({TInfo.ForwardSlash}{TInfo.ClassProperty})*", list =>
           {
               var itemList = new Stack<Vertex>();
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
               QueueItem propertyTree = new QueueItem(new Vertex(new TextInfo(vertex.Value.Text, vertex.Value.Value, TInfo.ExpandProperty)));

               // build from linear list, the tree
               for (vertex = itemList.Any() ? itemList.Peek() : null; itemList.Count != 0; vertex = itemList.Pop())
               {
                   vertex = new Vertex(new TextInfo(vertex.Value.Text, vertex.Value.Value, TInfo.ExpandProperty));
                   var edge = new Edge(vertex, propertyTree.Root);
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
                var children = new List<ITuple<Edge, QueueItem>>(grouped.Children)
                {
                    ITuple.Create(new Edge(grouped.Root, expand.Root), expand)
                };

                return ITuple.Create(new QueueItem(grouped.Root, children), 2);
            }),
            Create(TInfo.ExpandProperty + TInfo.Comma + TInfo.ListOfExpands, list =>
            {
                var filtered = list.Where(x => x.Root.Value.Representation != TInfo.Comma).ToList();
                var grouped = filtered[1];
                var expand = filtered[0];
                var children = new List<ITuple<Edge, QueueItem>>(grouped.Children)
                {
                    ITuple.Create(new Edge(grouped.Root, expand.Root), expand)
                };

                return ITuple.Create(new QueueItem(grouped.Root, children), 2);
            }),
            Create(TInfo.ListOfExpands + TInfo.Comma + TInfo.ListOfExpands, list =>
            {
                var filtered = list.Where(x => x.Root.Value.Representation != TInfo.Comma).ToList();
                var grouped = filtered[0];
                var otherGrouped = filtered[1];
                var children = new List<ITuple<Edge, QueueItem>>(grouped.Children);
                foreach (var child in otherGrouped.Children)
                    children.Add(child);
                var raw = new List<string>();

                return ITuple.Create(new QueueItem(grouped.Root, children), 2);
            }),
            Create($"{TInfo.ExpandProperty}({TInfo.Comma}{TInfo.ExpandProperty})*", list =>
            {
                var filtered = list.Where(x => x.Representation == TInfo.ExpandProperty).ToList();
                var children = new Queue<QueueItem>(filtered);

                var root = new Vertex(TextInfo.FromRepresentation(TInfo.ListOfExpands));
                var edges = children.Select(t => new Edge(root, t.Root));
                var childrenItems = new List<ITuple<Edge, QueueItem>>();
                foreach (var child in children)
                    childrenItems.Add(ITuple.Create(edges.First(e => e.To == child.Root), child));

                return ITuple.Create(new QueueItem(root, childrenItems), (list.Count - filtered.Count) - 1);
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
                var root = new Vertex(TInfo.FromRepresentation(TInfo.ListOfClause, typeof(TInfo)));

                bool IsClause(QueueItem item)
                {
                    if (item.Representation == expand) return true;
                    if (item.Representation == filter) return true;
                    if (item.Representation == select) return true;
                    return false;
                }

                var childrenWithEdge = list.Where(IsClause).Select(x => ITuple.Create(new Edge(root, x.Root), x));
                return ITuple.Create(new QueueItem(root, childrenWithEdge), list.Count - 1);
            });
        }

        public static TGrouping FilterExpression = Create(TInfo.FilterClause + TInfo.BooleanValue, list =>
            {
                var root = new Vertex(TInfo.FromRepresentation(TInfo.FilterExpression));
                return ITuple.Create(new QueueItem(root, new[] { ITuple.Create(new Edge(root, list[1].Root), list[1]) }), list.Count - 1);
            });

        public static TGrouping SelectExpression = Create(TInfo.SelectClause + TInfo.ListOfExpands, list =>
            {
                var root = new Vertex(TInfo.FromRepresentation(TInfo.SelectExpression));
                return ITuple.Create(new QueueItem(root, new[] { ITuple.Create(new Edge(root, list[1].Root), list[1]) }), 1);
            });

        public static TGrouping ExpandExpression = Create(TInfo.ExpandClause + TInfo.ListOfExpands, list =>
            {
                var root = new Vertex(TInfo.FromRepresentation(TInfo.ExpandExpression));
                return ITuple.Create(new QueueItem(root, new[] { ITuple.Create(new Edge(root, list[1].Root), list[1]) }), 1);
            });

        public static TGrouping ExpandPropertyWithListOfClauses = Create($"{TInfo.ExpandProperty}{TInfo.OpenParenthesis}{TInfo.ListOfClause}?{TInfo.CloseParenthesis}", list =>
            {
                // add one to the children
                var root = list[0].Root;
                var children = list[0].Children.ToList();
                children.Add(ITuple.Create(new Edge(root, list[2].Root), list[2]));
                return ITuple.Create(new QueueItem(root, children), 3);
            });

    }
}
