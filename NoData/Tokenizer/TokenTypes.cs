namespace NoData.Internal.TreeParser.Tokenizer
{
    public enum TokenType
    {
        classProperties,

        quotedString,
        whitespace,

        parenthesis,

        truth,
        falsey,

        not,

        and,
        or,

        add,
        subtract,
        equals,
        notEquals,

        number,

        greaterThan,
        lessThan,
        greaterThanOrEqual,
        lessThanOrEqual,

        forwardSlash,
        comma,

        filterClause,
        expandClause,
        selectClause,
        semiColin,

        ascending,
        descending,

        // string functions
        strLength,
        strSubstring,
        strStartsWith,
        strEndsWith,
        strConcat,
        strContains,
        strIndexOf,
        strToLower,
        strToUpper,
        strTrim,
        strReplace,
    }
}
