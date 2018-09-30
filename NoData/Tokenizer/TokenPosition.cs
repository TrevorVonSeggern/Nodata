using System;
using System.Collections.Generic;
using System.Text;
using CodeTools;

namespace NoData.Internal.TreeParser.Tokenizer
{
    [Immutable]
    public class TokenPosition
    {
        public int Index { get; }
        public int EndIndex { get; }

        public TokenPosition(int index, int endIndex)
        {
            Index = index;
            EndIndex = endIndex;
        }
    }
}
