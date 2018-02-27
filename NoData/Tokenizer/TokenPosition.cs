using System;
using System.Collections.Generic;
using System.Text;

namespace NoData.Internal.TreeParser.Tokenizer
{
    public class TokenPosition
    {
        public int Column { get; set; }
        public int Index { get; set; }
        public int Line { get; set; }

        public TokenPosition(int column, int index, int line)
        {
            Column = column;
            Index = index;
            Line = line;
        }
    }
}
