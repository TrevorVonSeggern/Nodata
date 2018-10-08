namespace NoData.GraphImplementations.QueryParser
{
    public class RollingCharacterRegexRepresentation
    {
        private int index = 0;
        public char EscapeCharacter = '<';
        private static char[] possibleChars = new char[]
        {
            '0', '1', '2', '3', '4', '5', '6', '7', '8', '9',
            'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm',
            'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z',
            'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M',
            'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z',
        };
        public string NextRep()
        {
            int remainder = index % possibleChars.Length;
            int tenColumn = index / possibleChars.Length;
            ++index;
            var littleChar = possibleChars[remainder];
            if (tenColumn == 0)
                return $"{EscapeCharacter}{littleChar}";
            return $"{EscapeCharacter}{possibleChars[tenColumn - 1]}{littleChar}";
        }
        // public string NextRep(string regex) => Regex.Escape(regex); // more human readable, but slower, way of getting text representation.
        public string NextRep(string regex) => NextRep();
    }

    public static class TextRepresentation
    {
        private static readonly RollingCharacterRegexRepresentation ConstRep = new RollingCharacterRegexRepresentation();

        // value types
        public static readonly string RawTextRepresentation = ConstRep.NextRep("<raw_text>");
        public static readonly string ClassProperty = ConstRep.NextRep("<class_property>");
        public static readonly string TextValue = ConstRep.NextRep("<text>");
        public static readonly string BooleanValue = ConstRep.NextRep("<bool>");
        public static readonly string NumberValue = ConstRep.NextRep("<number>");
        public static readonly string DateValue = ConstRep.NextRep("<date>");

        // grouping
        public static readonly string LogicalComparison = ConstRep.NextRep("<and_or>");
        public static readonly string ValueComparison = ConstRep.NextRep("<value_comparison>");
        public static readonly string Add = ConstRep.NextRep("<add>");
        public static readonly string MathSymbols = ConstRep.NextRep("<math_operator>");
        public static readonly string Inverse = ConstRep.NextRep("<inverse_operator>");

        // Expansion
        public static readonly string ExpandProperty = ConstRep.NextRep("<expandProperty>");
        public static readonly string ListOfExpands = ConstRep.NextRep("<list_of_expand>");

        // Sorting
        public static readonly string SortProperty = ConstRep.NextRep("<sort_property>");
        public static readonly string ListOfSortings = ConstRep.NextRep("<list_of_sort>");
        public static readonly string SortOrder = ConstRep.NextRep("<sort_order>");

        // text symbols
        public static readonly string SemiColin = ConstRep.NextRep("<semi_colin>");
        public static readonly string ForwardSlash = ConstRep.NextRep("<forward_slash>");
        public static readonly string Comma = ConstRep.NextRep("<comma>");
        public static readonly string OpenParenthesis = ConstRep.NextRep("<open_grouping>");
        public static readonly string CloseParenthesis = ConstRep.NextRep("<close_grouping>");

        // sub-parameters
        public static readonly string SelectClause = ConstRep.NextRep("<select_clause>");
        public static readonly string ExpandClause = ConstRep.NextRep("<expand_clause>");
        public static readonly string FilterClause = ConstRep.NextRep("<filter_clause>");
        public static readonly string SelectExpression = ConstRep.NextRep("<select_expr>");
        public static readonly string ExpandExpression = ConstRep.NextRep("<expand_expr>");
        public static readonly string FilterExpression = ConstRep.NextRep("<filter_expr>");
        public static readonly string ListOfClause = ConstRep.NextRep("<list_of_clause>");

        // String functions
        public static readonly string StrLength = ConstRep.NextRep("<str_length>");
        public static readonly string StrSubString = ConstRep.NextRep("<str_substring>");
        public static readonly string StrStartsWith = ConstRep.NextRep("<str_starts_with>");
        public static readonly string StrEndsWith = ConstRep.NextRep("<str_ends_with>");
        public static readonly string StrConcat = ConstRep.NextRep("<str_concat>");
        public static readonly string StrContains = ConstRep.NextRep("<str_contains>");
        public static readonly string StrIndexOf = ConstRep.NextRep("<str_index_of>");
        public static readonly string StrToLower = ConstRep.NextRep("<str_to_lower>");
        public static readonly string StrToUpper = ConstRep.NextRep("<str_to_upper>");
        public static readonly string StrTrim = ConstRep.NextRep("<str_trim>");
        public static readonly string StrReplace = ConstRep.NextRep("<str_replace>");
    }
}