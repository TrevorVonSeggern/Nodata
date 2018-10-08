using System;
using System.Collections.Generic;
using System.Text;

namespace NoData.Internal.TreeParser.Tokenizer
{
    public enum TokenTypes
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
        str_length,
        str_substring,
        str_starts_with,
        str_ends_with,
        str_concat,
        str_contains,
        str_index_of,
        str_to_lower,
        str_to_upper,
        str_trim,
        str_replace,
    }
}
