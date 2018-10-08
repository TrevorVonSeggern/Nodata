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

            string StringFunctionRegexHelper(string fctn, int argCount)
            {
                var result = $"{fctn}{TextRepresentation.OpenParenthesis}";
                for (int i = 0; i < argCount; ++i)
                {
                    if (i == argCount - 1)
                        result += strTypeRegex;
                    else
                        result += $"{strTypeRegex}{TextRepresentation.Comma}";
                }
                return result + $"{TextRepresentation.CloseParenthesis}";
            }

            ITuple<QueueItem, int> strFunctionHelper(IList<QueueItem> list, string rootText, string rootRepresentation)
            {
                var totalCount = list.Count;
                var totalAfterWastedItems = totalCount - 2;
                var total = totalAfterWastedItems / 2;

                var root = new Vertex(new TInfo(rootText, rootRepresentation));

                var strArgs = new List<QueueItem>(total);
                for (var i = 0; i < total; ++i)
                    strArgs.Add(list[(i * 2) + 2]);

                var edgeArgs = new List<Edge>(total);
                foreach (var vertex in strArgs)
                    edgeArgs.Add(new Edge(root, vertex.Root));

                var children = new List<ITuple<Edge, QueueItem>>(total);
                for (var i = 0; i < strArgs.Count && i < edgeArgs.Count; ++i)
                    children.Add(ITuple.Create(edgeArgs[i], strArgs[i]));

                return ITuple.Create(new QueueItem(root, children), list.Count - 1);
            }

            var endsWithRegex = StringFunctionRegexHelper(TextRepresentation.StrEndsWith, 2);
            yield return Create(endsWithRegex, list => strFunctionHelper(list, TextRepresentation.StrEndsWith, TextRepresentation.BooleanValue));

            var startsWithRegex = StringFunctionRegexHelper(TextRepresentation.StrStartsWith, 2);
            yield return Create(startsWithRegex, list => strFunctionHelper(list, TextRepresentation.StrStartsWith, TextRepresentation.BooleanValue));

            var indexOfRegex = StringFunctionRegexHelper(TextRepresentation.StrIndexOf, 2);
            yield return Create(indexOfRegex, list => strFunctionHelper(list, TextRepresentation.StrIndexOf, TextRepresentation.NumberValue));

            var containsRegex = StringFunctionRegexHelper(TextRepresentation.StrContains, 2);
            yield return Create(containsRegex, list => strFunctionHelper(list, TextRepresentation.StrContains, TextRepresentation.BooleanValue));

            var replaceRegex = StringFunctionRegexHelper(TextRepresentation.StrReplace, 3);
            yield return Create(replaceRegex, list => strFunctionHelper(list, TextRepresentation.StrReplace, TextRepresentation.TextValue));

            var toLowerRegex = StringFunctionRegexHelper(TextRepresentation.StrToLower, 1);
            yield return Create(toLowerRegex, list => strFunctionHelper(list, TextRepresentation.StrToLower, TextRepresentation.TextValue));

            var toUpperRegex = StringFunctionRegexHelper(TextRepresentation.StrToUpper, 1);
            yield return Create(toUpperRegex, list => strFunctionHelper(list, TextRepresentation.StrToUpper, TextRepresentation.TextValue));

            var concatRegex = StringFunctionRegexHelper(TextRepresentation.StrConcat, 2);
            yield return Create(concatRegex, list => strFunctionHelper(list, TextRepresentation.StrConcat, TextRepresentation.TextValue));

            var trimRegex = StringFunctionRegexHelper(TextRepresentation.StrTrim, 1);
            yield return Create(trimRegex, list => strFunctionHelper(list, TextRepresentation.StrTrim, TextRepresentation.TextValue));

            string StringFunctionRegexHelperWithIntArgs(string fctn, int argCount)
            {
                var result = $"{fctn}{TextRepresentation.OpenParenthesis}";
                for (int i = 0; i < argCount; ++i)
                {
                    if (i == 0)
                        result += strTypeRegex;
                    else
                        result += TextRepresentation.NumberValue;

                    if (i != argCount - 1)
                        result += TextRepresentation.Comma;
                }
                return result + $"{TextRepresentation.CloseParenthesis}";
            }

            var substring1Regex = StringFunctionRegexHelperWithIntArgs(TextRepresentation.StrSubString, 2);
            yield return Create(substring1Regex, list => strFunctionHelper(list, TextRepresentation.StrSubString, TextRepresentation.TextValue));

            var substring2Regex = StringFunctionRegexHelperWithIntArgs(TextRepresentation.StrSubString, 3);
            yield return Create(substring2Regex, list => strFunctionHelper(list, TextRepresentation.StrSubString, TextRepresentation.TextValue));

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
