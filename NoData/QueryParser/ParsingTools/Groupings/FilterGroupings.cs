using NoData.Graph.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using QueueItem = NoData.QueryParser.Graph.Tree;
using TInfo = NoData.QueryParser.Graph.TextInfo;
using TGrouping = NoData.Graph.Base.ITuple<string, System.Func<System.Collections.Generic.IList<NoData.QueryParser.Graph.Tree>, NoData.Graph.Base.ITuple<NoData.QueryParser.Graph.Tree, int>>>;

namespace NoData.QueryParser.ParsingTools.Groupings
{
    static internal class FilterGroupings
    {
        private static TGrouping Create(string pattern, Func<IList<QueueItem>, ITuple<QueueItem, int>> addFunc) => ITuple.Create(pattern, addFunc);

        public static IEnumerable<TGrouping> AddTermsForFilter<TQueryVertex>(NoData.Graph.Graph graph)
        {
            // Take an expand property, and make it a value type.
            yield return  Create(TInfo.ExpandProperty, list =>
            {
                var item = list[0];
                var propertyNameList = new List<string>();

                item.Traverse((Graph.Vertex e) =>
                {
                    propertyNameList.Add(e.Value.Value);
                });

                var propInfo = NoData.Graph.Utility.GraphUtility.GetPropertyFromPathString(string.Join("/", propertyNameList), typeof(TQueryVertex), graph);
                if (propInfo is null) return null;
                var rep = TInfo.RawTextRepresentation;
                var type = propInfo.PropertyType;
                if (type == typeof(Int16) ||
                    type == typeof(Int32) ||
                    type == typeof(long) ||
                    type == typeof(double) ||
                    type == typeof(float) ||
                    type == typeof(decimal))
                    rep = TInfo.NumberValue;
                else if (type == typeof(DateTime) || type == typeof(DateTimeOffset))
                    rep = TInfo.DateValue;
                else if (type == typeof(string))
                    rep = TInfo.TextValue;

                var root = new Graph.Vertex(new TInfo { Value = propInfo.PropertyType.Name, Text = propInfo.Name, Representation = rep, Type = type });
                var child = item;
                var edge = new Graph.Edge(root, child.Root);
                return ITuple.Create(new QueueItem(root, new[] { ITuple.Create(edge, child) }, list[0].RawText), 0);
            });

            yield return Create(TInfo.Inverse + TInfo.BooleanValue, list =>
            {
                var root = new Graph.Vertex(new TInfo { Value = "!", Representation = TInfo.BooleanValue });
                var child = list[1];
                var edge = new Graph.Edge(root, child.Root);
                var raw = new List<string>();
                raw.AddRange(list[0].RawText);
                raw.AddRange(child.RawText);
                return ITuple.Create(new QueueItem(root, new[] { ITuple.Create(edge, new QueueItem(child.Root, child.RawText)) }, raw), 1);
            });

            ITuple<QueueItem, int> valueItemValue(IList<QueueItem> list)
            {
                var root = new Graph.Vertex(new TInfo { Value = list[1].Root.Value.Representation, Text = list[1].Root.Value.Text, Representation = TInfo.BooleanValue });
                var left = list[0];
                var right = list[2];
                var edgeLeft = new Graph.Edge(root, left.Root);
                var edgeRight = new Graph.Edge(root, right.Root);
                var raw = new List<string>();
                raw.AddRange(left.RawText);
                raw.Add(root.Value.Text);
                raw.AddRange(right.RawText);
                return ITuple.Create(new QueueItem(root, new[] {
                    ITuple.Create(edgeLeft, left),
                    ITuple.Create(edgeRight, right),
                }, raw), 2);
            }

            string valueComparisonPattern(string a, string b) => a + TInfo.ValueComparison + b;
            yield return Create(valueComparisonPattern(TInfo.BooleanValue, TInfo.BooleanValue), valueItemValue);
            yield return Create(valueComparisonPattern(TInfo.TextValue, TInfo.TextValue), valueItemValue);
            yield return Create(valueComparisonPattern(TInfo.NumberValue, TInfo.NumberValue), valueItemValue);
            yield return Create(valueComparisonPattern(TInfo.DateValue, TInfo.DateValue), valueItemValue);

            ITuple<QueueItem, int> undoGrouping(IList<QueueItem> list) => ITuple.Create(list[1], 2);
            string anyValueTypeRegex = $"({TInfo.BooleanValue}|{TInfo.NumberValue}|{TInfo.TextValue}|{TInfo.DateValue})";
            yield return Create(TInfo.OpenParenthesis + anyValueTypeRegex + TInfo.CloseParenthesis, undoGrouping);

            string logicComparisonPattern(string a, string b) => $"{a}{TInfo.LogicalComparison}{b}";
            yield return Create(logicComparisonPattern(TInfo.BooleanValue, TInfo.BooleanValue), valueItemValue);
        }
    }
}
