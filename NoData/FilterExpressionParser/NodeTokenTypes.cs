namespace NoData.Internal.TreeParser.FilterExpressionParser
{
    public enum NodeTokenTypes
    {
        ValueComparedToValue,
        ValueOperationValue,
        ValueAndOrValue,
        InverseOfValue,
        GroupValueGroup,
        Value,
        Operator,
        Inverse,
        Comparator,
        Property,
        LogicalOperator
    }
}
