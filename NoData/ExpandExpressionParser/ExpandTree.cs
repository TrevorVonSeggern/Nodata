using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace NoData.Internal.TreeParser.ExpandExpressionParser
{
    using Nodes;
    using NoData.Internal.TreeParser.Nodes;
    using Tokenizer;
    using NoData.Utility;

    public class ExpandTree<TDto> where TDto : class
    {
        public Node Root { get; set; }

        public ExpandTree() { }

        public Node ParseExpand (string expandString)
        {
            var sourceTokenizer = new Tokenizer(typeof(TDto).GetProperties().Select(x => x.Name));

            // add tokens
            var queue = new List<Node>(sourceTokenizer.Tokenize(expandString).Select(x => new NodePlaceHolder(' ', x)));

            var nodeTokenizer = new NodeGrouper();
            var foundMatch = true;
            while (foundMatch)
            {
                foundMatch = false;
                var token = nodeTokenizer.Tokenize(NodeGrouper.GetQueueRepresentationalString(queue));
                if (token is null)
                    break;
                var one = token.Position.Index;
                var two = one + 1;
                var three = one + 2;
                if (token?.Value == null)
                    break;
                if (token.Value.Length == 0)
                    break;

                if (!Enum.TryParse(token.Type, out NodeTokenTypes type))
                    break;

                if(type == NodeTokenTypes.ExpandProperty)
                {
                    foundMatch = true;
                    var itemList = new List<Node>();
                    itemList.Add(queue[one]);
                    for(int i = 0; i < token.Value.Length / 2; ++i)
                    {
                        queue.RemoveAt(two); // delete the slash.
                        itemList.Add(queue[two]);
                        queue.RemoveAt(two);
                    }
                    var item = new NodeExpandProperty<TDto>(itemList);
                    queue[one] = item;
                }
                if(type == NodeTokenTypes.ExpandCollection)
                {
                    foundMatch = true;
                    var itemList = new List<NodeExpandProperty<TDto>>();
                    itemList.Add(queue[one] as NodeExpandProperty<TDto>);
                    for(int i = 0; i < token.Value.Length / 2; ++i)
                    {
                        queue.RemoveAt(two); // delete the comma.
                        itemList.Add(queue[two] as NodeExpandProperty<TDto>);
                        queue.RemoveAt(two);
                    }
                    var item = new NodeExpandCollection<TDto>(itemList);
                    queue[one] = item;
                }
            }
            if (queue.Count == 0)
            {
                var placeholder = new NodePlaceHolder(' ', new Token(TokenTypes.classProperties, "", new TokenPosition(0, 0)));
                Root = new NodeExpandProperty<TDto>(placeholder);
            }
            else if (queue.Count == 1)
            {
                Root = queue[0];
                return Root;
            }
            return null;
        }

        public IQueryable<TDto> ApplyExpand(IQueryable<TDto> query)
        {
            var parameter = Expression.Parameter(typeof(TDto));
            var body = Root?.GetExpression(parameter) ?? throw new ArgumentNullException("Root filter node or resulting expression is null");

            var expr = ExpressionBuilder.BuildSelectExpression(query, parameter, body);

            return query.Provider.CreateQuery<TDto>(expr);
        }
    }
}
