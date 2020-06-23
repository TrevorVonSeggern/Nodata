namespace NoData.GraphImplementations.QueryParser
{
	// The purpose of this class is to give a specific character (alpha numeric) which represents the key to a regular expression.
    public class RollingCharacterRegexRepresentation
    {
        private int index = 0;
        private char EscapeCharacter { get; } = '<';
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
            var remainder = index % possibleChars.Length;
            var tenColumn = index / possibleChars.Length;
            ++index;
            var littleChar = possibleChars[remainder];
            if (tenColumn == 0)
                return $"{EscapeCharacter}{littleChar}";
            return $"{EscapeCharacter}{possibleChars[tenColumn - 1]}{littleChar}";
        }
        public string NextRep(string regex) => regex is null ? null : NextRep();
        // public string NextRep(string regex) => System.Text.RegularExpressions.Regex.Escape(regex); // more human readable, but slower, way of getting text representation.
    }

    public static class TextRepresentation
    {
        private static RollingCharacterRegexRepresentation ConstRep { get; } = new RollingCharacterRegexRepresentation();

        // value types
        public static string RawTextRepresentation { get; } = ConstRep.NextRep("<raw_text>");
        public static string ClassProperty { get; } = ConstRep.NextRep("<class_property>");
        public static string TextValue { get; } = ConstRep.NextRep("<text>");
        public static string BooleanValue { get; } = ConstRep.NextRep("<bool>");
        public static string NumberValue { get; } = ConstRep.NextRep("<number>");
        public static string DateValue { get; } = ConstRep.NextRep("<date>");

        // grouping
        public static string LogicalComparison { get; } = ConstRep.NextRep("<and_or>");
        public static string ValueComparison { get; } = ConstRep.NextRep("<value_comparison>");
        public static string Add { get; } = ConstRep.NextRep("<add>");
        public static string MathSymbols { get; } = ConstRep.NextRep("<math_operator>");
        public static string Inverse { get; } = ConstRep.NextRep("<inverse_operator>");

        // Expansion
        public static string ExpandProperty { get; } = ConstRep.NextRep("<expandProperty>");
        public static string ListOfExpands { get; } = ConstRep.NextRep("<list_of_expand>");

        // Sorting
        public static string SortProperty { get; } = ConstRep.NextRep("<sort_property>");
        public static string ListOfSortings { get; } = ConstRep.NextRep("<list_of_sort>");
        public static string SortOrder { get; } = ConstRep.NextRep("<sort_order>");

        // text symbols
        public static string SemiColin { get; } = ConstRep.NextRep("<semi_colin>");
        public static string ForwardSlash { get; } = ConstRep.NextRep("<forward_slash>");
        public static string Comma { get; } = ConstRep.NextRep("<comma>");
        public static string OpenParenthesis { get; } = ConstRep.NextRep("<open_grouping>");
        public static string CloseParenthesis { get; } = ConstRep.NextRep("<close_grouping>");

        // sub-parameters
        public static string SelectClause { get; } = ConstRep.NextRep("<select_clause>");
        public static string ExpandClause { get; } = ConstRep.NextRep("<expand_clause>");
        public static string FilterClause { get; } = ConstRep.NextRep("<filter_clause>");
        public static string SelectExpression { get; } = ConstRep.NextRep("<select_expr>");
        public static string ExpandExpression { get; } = ConstRep.NextRep("<expand_expr>");
        public static string FilterExpression { get; } = ConstRep.NextRep("<filter_expr>");
        public static string ListOfClause { get; } = ConstRep.NextRep("<list_of_clause>");

        // String functions
        public static string StrLength { get; } = ConstRep.NextRep("<str_length>");
        public static string StrSubString { get; } = ConstRep.NextRep("<str_substring>");
        public static string StrStartsWith { get; } = ConstRep.NextRep("<str_starts_with>");
        public static string StrEndsWith { get; } = ConstRep.NextRep("<str_ends_with>");
        public static string StrConcat { get; } = ConstRep.NextRep("<str_concat>");
        public static string StrContains { get; } = ConstRep.NextRep("<str_contains>");
        public static string StrIndexOf { get; } = ConstRep.NextRep("<str_index_of>");
        public static string StrToLower { get; } = ConstRep.NextRep("<str_to_lower>");
        public static string StrToUpper { get; } = ConstRep.NextRep("<str_to_upper>");
        public static string StrTrim { get; } = ConstRep.NextRep("<str_trim>");
        public static string StrReplace { get; } = ConstRep.NextRep("<str_replace>");
    }
}
