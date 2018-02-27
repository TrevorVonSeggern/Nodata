using System;
using System.Collections.Generic;
using System.Text;
using NoData.Internal.TreeParser.Tokenizer;
using System.Reflection;
using System.Linq;
using System.Linq.Expressions;

namespace NoData.Internal.TreeParser.BinaryTreeParser
{
    internal static class NodeExtensions
    {
        private static readonly TokenTypes[] _ValueTokenTypes = new TokenTypes[]
        {
            TokenTypes.classProperties,
            TokenTypes.number,
            TokenTypes.quotedString,
        };
        public static bool IsValueTypeBacking(this Node node)
        {
            if (node == null)
                return false;
            if (node.Token is null)
                return false;
            if (node is LogicalOperatorNode || node is PropertyNode || node is ComparitorNode)
                return true;
            foreach (var type in _ValueTokenTypes)
                if (node.Token.Type == type.ToString())
                    return true;
            // TODO: Add support for functions here.
            //if (node.Token.Type == TokenDefinition.TokenTypes.function)
            //    return true;
            return false;
        }
        private static readonly TokenTypes[] _ComparitorTokenTypes = new TokenTypes[]
        {
            TokenTypes.greaterThan,
            TokenTypes.greaterThanOrEqual,
            TokenTypes.lessThan,
            TokenTypes.lessThanOrEqual,
            TokenTypes.equals,
            TokenTypes.notEquals,
        };
        public static bool IsComparitor(this Node node)
        {
            if (node == null)
                return false;
            if (node.Token is null)
                return false;
            if (node is ComparitorNode)
                return true;
            foreach (var type in _ComparitorTokenTypes)
                if (node.Token.Type == type.ToString())
                    return true;
            return false;
        }

        private static readonly TokenTypes[] _LogicalTokenTypes = new TokenTypes[]
        {
            TokenTypes.and,
            TokenTypes.or,
        };
        public static bool IsLogicalComparitor(this Node node)
        {
            if (node == null)
                return false;
            if (node.Token is null)
                return false;
            if (node is LogicalOperatorNode)
                return true;
            foreach (var type in _LogicalTokenTypes)
                if (node.Token.Type == type.ToString())
                    return true;
            return false;
        }
    }


    public class FilterTree<TDto> where TDto : class, new()
    {
        public Node Root { get; set; }

        private bool IsComaritorCheck(Node left, Node middle, Node right)
        {
            if (left.IsValueTypeBacking() && middle.IsComparitor() && right.IsValueTypeBacking())
                return true;
            return false;
        }

        private bool IsLogicalOperatorCheck(Node left, Node middle, Node right)
        {
            if (left.IsValueTypeBacking() && middle.IsLogicalComparitor() && right.IsValueTypeBacking())
                return true;
            return false;
        }

        public FilterTree(string sourceFilter)
        {
            Type type = typeof(TDto);
            var sourceTokenizer = new Tokenizer.Tokenizer(type.GetProperties().Select(x => x.Name));

            // add tokens
            var queue = new List<Node>(sourceTokenizer.Tokenize(sourceFilter).Select(x => new NodePlaceHolder(' ', x)));

            // replace nodes with properties if needed.
            for (int p = 0; p < queue.Count(); ++p)
            {
                if (queue[p].Token.Type == TokenTypes.classProperties.ToString())
                    queue[p] = new PropertyNode(queue[p]);
                else if (queue[p].Token.Type == TokenTypes.quotedString.ToString() || queue[p].Token.Type == TokenTypes.number.ToString())
                    queue[p] = new ConstantValueNode(queue[p]);
            }

            var nodeTokenizer = new NodeGrouper();
            while (queue.Count() > 1)
            {
                var foundMatch = false;
                var token = nodeTokenizer.Tokenize(GetQueueRepresentationalString(queue));
                if (token is null)
                    break;
                var one = token.Position.Column;
                var two = one + 1;
                var three = one + 2;
                if (token?.Value == null)
                    break;
                if (token.Value.Length <= 1)
                    break;
                if (token.Value.Length == 2 && token.Type == NodeTokenTypes.InverseOfValue.ToString())
                {
                    foundMatch = true;
                    var inverseNode = new InverseOperatorNode(queue[one], queue[two]);
                    queue[one] = inverseNode;
                    queue.RemoveAt(two);
                }
                else if (token.Value.Length == 3)
                {
                    if (token.Type == NodeTokenTypes.ValueComparedToValue.ToString())
                    {
                        foundMatch = true;
                        var comparitorNode = new ComparitorNode(queue[one], queue[two], queue[three]);
                        queue[one] = comparitorNode;
                        queue.RemoveAt(three);
                        queue.RemoveAt(two);
                    }
                    else if (token.Type == NodeTokenTypes.ValueAndOrValue.ToString())
                    {
                        foundMatch = true;
                        var comparitorNode = new LogicalOperatorNode(queue[one], queue[two], queue[three]);
                        queue[one] = comparitorNode;
                        queue.RemoveAt(three);
                        queue.RemoveAt(two);
                    }
                    else if (token.Type == NodeTokenTypes.GroupValueGroup.ToString())
                    {
                        foundMatch = true;
                        queue.RemoveAt(three);
                        queue.RemoveAt(one);
                    }
                }
                if (!foundMatch)
                    return;
            }
            if (queue.Count == 1)
                Root = queue[0];
        }

        private string GetQueueRepresentationalString(List<Node> queue)
            => string.Join("", queue.Select(x => GetNodeRepresentation(x)));

        private string GetNodeRepresentation(Node n)
        {
            if (n.GetType() == typeof(NodePlaceHolder))
            {
                var t = n?.Token?.Type;
                if (t == null)
                    return " ";
                if (t == TokenTypes.equals.ToString() ||
                    t == TokenTypes.notEquals.ToString() ||
                    t == TokenTypes.greaterThan.ToString() ||
                    t == TokenTypes.greaterThanOrEqual.ToString() ||
                    t == TokenTypes.lessThan.ToString() ||
                    t == TokenTypes.lessThanOrEqual.ToString())
                    return "c";
                if (t == TokenTypes.not.ToString())
                    return "!";
                if (t == TokenTypes.number.ToString() ||
                    t == TokenTypes.quotedString.ToString())
                    return "v";
                if (t == TokenTypes.subtract.ToString() ||
                    t == TokenTypes.add.ToString())
                    return "o";
                if (t == TokenTypes.and.ToString() ||
                    t == TokenTypes.or.ToString())
                    return "l";
                if (t == TokenTypes.parenthesis.ToString())
                    return n.Token.Value;
                if (t == TokenTypes.whitespace.ToString())
                    return " ";
            }
            return n?.Representation.ToString() ?? "#";
        }

        public IQueryable<TDto> ApplyFilter(IQueryable<TDto> query)
        {
            var parameter = Expression.Parameter(typeof(TDto));
            var expression = Root?.GetExpression(parameter) ?? throw new ArgumentNullException("Root filter node or resulting expression is null");

            var expr = Expression.Call(
                typeof(Queryable),
                "Where",
                new[] { typeof(TDto) },
                query.Expression,
                Expression.Lambda(expression, parameter)
            );

            return query.Provider.CreateQuery<TDto>(expr);
        }
    }
}
