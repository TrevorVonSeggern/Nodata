using System;
using System.Collections.Generic;
using System.Text;

namespace NoData.Internal.TreeParser.Tokenizer
{
    public class TokenPosition
    {
        public int Index { get; set; }
        public int EndIndex { get; set; }

        public TokenPosition(int index, int endIndex)
        {
            Index = index;
            EndIndex = endIndex;
        }
    }
}
