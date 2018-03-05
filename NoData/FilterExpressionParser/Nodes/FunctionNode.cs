using NoData.Internal.TreeParser.Nodes;
using System;
using System.Linq.Expressions;

namespace NoData.Internal.TreeParser.FilterExpressionParser.Nodes
{
    /// <summary>
    ///  Function Node. This represends a function on one of the many functions that OData has for strings, dates, etc.
    /// </summary>
    public class FunctionNode : Node
    {
        public FunctionNode() : base('v')
        {

        }

        public override Expression GetExpression(ParameterExpression dto)
        {
            throw new NotImplementedException();
        }
    }
}
