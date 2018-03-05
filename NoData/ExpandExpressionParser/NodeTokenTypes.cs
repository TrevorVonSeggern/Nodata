namespace NoData.Internal.TreeParser.ExpandExpressionParser
{
    public enum NodeTokenTypes
    {
        ExpandProperty,
        ExpandCollection,
        Property,
    }
    public static class NodeTokenUtilities
    {
        public static char GetCharacterFromType(NodeTokenTypes type)
        {
            switch(type)
            {
                default:
                    return ' ';
                case NodeTokenTypes.ExpandProperty:
                    return 'e';
                case NodeTokenTypes.ExpandCollection:
                    return 'c';
                case NodeTokenTypes.Property:
                    return 'p';
            }
        }
    }
}
