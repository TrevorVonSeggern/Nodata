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
            yield return Create(TextRepresentation.Inverse + $"({TextRepresentation.BooleanValue}|{TextRepresentation.ExpandProperty})", list =>
            {
                var root = new Vertex(new TInfo("!", TextRepresentation.BooleanValue));
                var child = list[1];
                var edge = new Edge(root, child.Root);
                return ITuple.Create(new QueueItem(root, new[] { ITuple.Create(edge, new QueueItem(child.Root)) }), 1);
            });

            ITuple<QueueItem, int> valueSomethingValue(IList<QueueItem> list, string rootText = null)
            {
                if (rootText is null)
                    rootText = TextRepresentation.ValueComparison;
                var root = new Vertex(new TInfo(rootText, TextRepresentation.BooleanValue));
                var left = list[0];
                var comparitor = list[1];
                var right = list[2];
                var edgeLeft = new Edge(root, left.Root);
                var edgeMiddle = new Edge(root, comparitor.Root);
                var edgeRight = new Edge(root, right.Root);
                return ITuple.Create(new QueueItem(root, new[] {
                    ITuple.Create(edgeLeft, left),
                    ITuple.Create(edgeMiddle, comparitor),
                    ITuple.Create(edgeRight, right),
                }), 2);
            }
            ITuple<QueueItem, int> valueItemValue(IList<QueueItem> list) => valueSomethingValue(list);

            string valueComparisonPattern(string a, string b) => a + TextRepresentation.ValueComparison + b;
            // value to value
            yield return Create(valueComparisonPattern(TextRepresentation.BooleanValue, TextRepresentation.BooleanValue), valueItemValue);
            yield return Create(valueComparisonPattern(TextRepresentation.TextValue, TextRepresentation.TextValue), valueItemValue);
            yield return Create(valueComparisonPattern(TextRepresentation.NumberValue, TextRepresentation.NumberValue), valueItemValue);
            yield return Create(valueComparisonPattern(TextRepresentation.DateValue, TextRepresentation.DateValue), valueItemValue);

            // property to value
            yield return Create(valueComparisonPattern(TextRepresentation.ExpandProperty, TextRepresentation.BooleanValue), valueItemValue);
            yield return Create(valueComparisonPattern(TextRepresentation.ExpandProperty, TextRepresentation.TextValue), valueItemValue);
            yield return Create(valueComparisonPattern(TextRepresentation.ExpandProperty, TextRepresentation.NumberValue), valueItemValue);
            yield return Create(valueComparisonPattern(TextRepresentation.ExpandProperty, TextRepresentation.DateValue), valueItemValue);

            // value to property
            yield return Create(valueComparisonPattern(TextRepresentation.BooleanValue, TextRepresentation.ExpandProperty), valueItemValue);
            yield return Create(valueComparisonPattern(TextRepresentation.TextValue, TextRepresentation.ExpandProperty), valueItemValue);
            yield return Create(valueComparisonPattern(TextRepresentation.NumberValue, TextRepresentation.ExpandProperty), valueItemValue);
            yield return Create(valueComparisonPattern(TextRepresentation.DateValue, TextRepresentation.ExpandProperty), valueItemValue);

            // property to property
            yield return Create(valueComparisonPattern(TextRepresentation.ExpandProperty, TextRepresentation.ExpandProperty), valueItemValue);

            string anyValueTypeRegex = $"({TextRepresentation.BooleanValue}|{TextRepresentation.NumberValue}|{TextRepresentation.TextValue}|{TextRepresentation.DateValue}|{TextRepresentation.ExpandProperty})";
            string strTypeRegex = $"({TextRepresentation.TextValue}|{TextRepresentation.ExpandProperty})";

            // string functions
            // length(x)
            var lenRegex = $"{TextRepresentation.StrLength}{TextRepresentation.OpenParenthesis}{strTypeRegex}{TextRepresentation.CloseParenthesis}";
            yield return Create(lenRegex, list =>
            {
                var root = new Vertex(new TInfo(TextRepresentation.StrLength, TextRepresentation.NumberValue));
                var lengthTree = list[2];
                var edgeRight = new Edge(root, lengthTree.Root);
                return ITuple.Create(new QueueItem(root, new[] {
                    ITuple.Create(edgeRight, lengthTree),
                }), 3);
            });
            // endswith(x, y)
            var endsWithRegex = $"{TextRepresentation.StrEndsWith}{TextRepresentation.OpenParenthesis}" +
                                $"{strTypeRegex}{TextRepresentation.Comma}{strTypeRegex}" +
                                $"{TextRepresentation.CloseParenthesis}";
            yield return Create(endsWithRegex, list =>
            {
                var root = new Vertex(new TInfo(TextRepresentation.StrEndsWith, TextRepresentation.BooleanValue));
                var strArg1 = list[2];
                var strArg2 = list[4];
                var edgeArg1 = new Edge(root, strArg1.Root);
                var edgeArg2 = new Edge(root, strArg2.Root);
                return ITuple.Create(new QueueItem(root, new[] {
                    ITuple.Create(edgeArg1, strArg1),
                    ITuple.Create(edgeArg2, strArg2),
                }), 5);
            });
            // startswith(x, y)
            var startsWithRegex = $"{TextRepresentation.StrStartsWith}{TextRepresentation.OpenParenthesis}" +
                                $"{strTypeRegex}{TextRepresentation.Comma}{strTypeRegex}" +
                                $"{TextRepresentation.CloseParenthesis}";
            yield return Create(startsWithRegex, list =>
            {
                var root = new Vertex(new TInfo(TextRepresentation.StrStartsWith, TextRepresentation.BooleanValue));
                var strArg1 = list[2];
                var strArg2 = list[4];
                var edgeArg1 = new Edge(root, strArg1.Root);
                var edgeArg2 = new Edge(root, strArg2.Root);
                return ITuple.Create(new QueueItem(root, new[] {
                    ITuple.Create(edgeArg1, strArg1),
                    ITuple.Create(edgeArg2, strArg2),
                }), 5);
            });
            // indexOf(x, y)
            var indexOfRegex = $"{TextRepresentation.StrIndexOf}{TextRepresentation.OpenParenthesis}" +
                                $"{strTypeRegex}{TextRepresentation.Comma}{strTypeRegex}" +
                                $"{TextRepresentation.CloseParenthesis}";
            yield return Create(indexOfRegex, list =>
            {
                var root = new Vertex(new TInfo(TextRepresentation.StrIndexOf, TextRepresentation.NumberValue));
                var strArg1 = list[2];
                var strArg2 = list[4];
                var edgeArg1 = new Edge(root, strArg1.Root);
                var edgeArg2 = new Edge(root, strArg2.Root);
                return ITuple.Create(new QueueItem(root, new[] {
                    ITuple.Create(edgeArg1, strArg1),
                    ITuple.Create(edgeArg2, strArg2),
                }), 5);
            });

            // ( )
            ITuple<QueueItem, int> undoGrouping(IList<QueueItem> list) => ITuple.Create(list[1], 2);
            yield return Create(TextRepresentation.OpenParenthesis + anyValueTypeRegex + TextRepresentation.CloseParenthesis, undoGrouping);

            // Logical comparisons. Ie and or not, etc.
            string logicComparisonPattern(string a, string b) => $"{a}{TextRepresentation.LogicalComparison}{b}";
            ITuple<QueueItem, int> valueLogicValue(IList<QueueItem> list) => valueSomethingValue(list, TextRepresentation.LogicalComparison);
            yield return Create(logicComparisonPattern(TextRepresentation.BooleanValue, TextRepresentation.BooleanValue), valueLogicValue);
            yield return Create(logicComparisonPattern(TextRepresentation.ExpandProperty, TextRepresentation.BooleanValue), valueLogicValue);
            yield return Create(logicComparisonPattern(TextRepresentation.BooleanValue, TextRepresentation.ExpandProperty), valueLogicValue);
            yield return Create(logicComparisonPattern(TextRepresentation.ExpandProperty, TextRepresentation.ExpandProperty), valueLogicValue);
        }
    }
}
