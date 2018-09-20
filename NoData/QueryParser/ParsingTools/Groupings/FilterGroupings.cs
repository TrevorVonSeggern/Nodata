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
    public static class FilterGroupings
    {
        private static TGrouping Create(string pattern, Func<IList<QueueItem>, ITuple<QueueItem, int>> addFunc) => ITuple.Create(pattern, addFunc);

        public static IEnumerable<TGrouping> AddTermsForFilter()
        {
            yield return Create(TInfo.Inverse + $"({TInfo.BooleanValue}|{TInfo.ExpandProperty})", list =>
            {
                var root = new Vertex(new TInfo { Value = "!", Representation = TInfo.BooleanValue });
                var child = list[1];
                var edge = new Edge(root, child.Root);
                return ITuple.Create(new QueueItem(root, new[] { ITuple.Create(edge, new QueueItem(child.Root)) }), 1);
            });

            ITuple<QueueItem, int> valueItemValue(IList<QueueItem> list)
            {
                var root = new Vertex(new TInfo { Value = list[1].Root.Value.Representation, Text = list[1].Root.Value.Text, Representation = TInfo.BooleanValue });
                var left = list[0];
                var right = list[2];
                var edgeLeft = new Edge(root, left.Root);
                var edgeRight = new Edge(root, right.Root);
                return ITuple.Create(new QueueItem(root, new[] {
                    ITuple.Create(edgeLeft, left),
                    ITuple.Create(edgeRight, right),
                }), 2);
            }

            string valueComparisonPattern(string a, string b) => a + TInfo.ValueComparison + b;
            yield return Create(valueComparisonPattern(TInfo.BooleanValue, TInfo.BooleanValue), valueItemValue);
            yield return Create(valueComparisonPattern(TInfo.TextValue, TInfo.TextValue), valueItemValue);
            yield return Create(valueComparisonPattern(TInfo.NumberValue, TInfo.NumberValue), valueItemValue);
            yield return Create(valueComparisonPattern(TInfo.DateValue, TInfo.DateValue), valueItemValue);

            yield return Create(valueComparisonPattern(TInfo.ExpandProperty, TInfo.BooleanValue), valueItemValue);
            yield return Create(valueComparisonPattern(TInfo.ExpandProperty, TInfo.TextValue), valueItemValue);
            yield return Create(valueComparisonPattern(TInfo.ExpandProperty, TInfo.NumberValue), valueItemValue);
            yield return Create(valueComparisonPattern(TInfo.ExpandProperty, TInfo.DateValue), valueItemValue);

            yield return Create(valueComparisonPattern(TInfo.BooleanValue, TInfo.ExpandProperty), valueItemValue);
            yield return Create(valueComparisonPattern(TInfo.TextValue, TInfo.ExpandProperty), valueItemValue);
            yield return Create(valueComparisonPattern(TInfo.NumberValue, TInfo.ExpandProperty), valueItemValue);
            yield return Create(valueComparisonPattern(TInfo.DateValue, TInfo.ExpandProperty), valueItemValue);

            yield return Create(valueComparisonPattern(TInfo.ExpandProperty, TInfo.ExpandProperty), valueItemValue);

            ITuple<QueueItem, int> undoGrouping(IList<QueueItem> list) => ITuple.Create(list[1], 2);
            string anyValueTypeRegex = $"({TInfo.BooleanValue}|{TInfo.NumberValue}|{TInfo.TextValue}|{TInfo.DateValue}|{TInfo.ExpandProperty})";
            yield return Create(TInfo.OpenParenthesis + anyValueTypeRegex + TInfo.CloseParenthesis, undoGrouping);

            string logicComparisonPattern(string a, string b) => $"{a}{TInfo.LogicalComparison}{b}";
            yield return Create(logicComparisonPattern(TInfo.BooleanValue, TInfo.BooleanValue), valueItemValue);
            yield return Create(logicComparisonPattern(TInfo.ExpandProperty, TInfo.BooleanValue), valueItemValue);
            yield return Create(logicComparisonPattern(TInfo.BooleanValue, TInfo.ExpandProperty), valueItemValue);
            yield return Create(logicComparisonPattern(TInfo.ExpandProperty, TInfo.ExpandProperty), valueItemValue);
        }
    }
}
