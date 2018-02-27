namespace NoData.Internal.TreeParser.BinaryTreeParser
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
