using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace NoData.Internal.TreeParser.BinaryTreeParser
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
