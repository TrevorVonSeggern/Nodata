using System;
using System.Collections.Generic;
using System.Text;

namespace NoData.Internal.TreeParser.Tokenizer
{
    public enum TokenTypes
    {
        classProperties,
        quotedString,
        number,
        parenthesis,
        not,
        and,
        or,
        add,
        subtract,
        equals,
        notEquals,
        whitespace,

        greaterThan,
        lessThan,
        greaterThanOrEqual,
        lessThanOrEqual,

        forwardSlash,
        comma,
    }
}
