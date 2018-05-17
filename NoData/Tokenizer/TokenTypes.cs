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
    }
}
