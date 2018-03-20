using System.Collections.Generic;
using System.Linq.Expressions;

namespace NoData.Internal.TreeParser.Nodes
{
    /// <summary>
    /// Base class for all things represented in a filter.
    /// </summary>
    public abstract class Node
    {
        public readonly char Representation = ' ';

        public List<Node> Children { get; set; }

        public Tokenizer.Token Token { get; set; }

        public abstract Expression GetExpression(Expression dto);

        public Node(char v, Tokenizer.Token token): this (v)
        {
            Children = new List<Node>();
            Token = token;
        }

        public Node(char v)
        {
            Children = new List<Node>();
            Representation = v;
        }
    }
}
