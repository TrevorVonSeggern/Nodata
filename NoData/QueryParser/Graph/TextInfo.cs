using NoData.Graph.Interfaces;
using NoData.Internal.TreeParser.Tokenizer;
using System;
using System.Text.RegularExpressions;

namespace NoData.QueryParser.Graph
{
    public class TextInfo : IMergable<TextInfo>
    {
        public string Text { get; set; }
        public string Value { get; set; }
        public string Representation { get; set; }
        public Type Type { get; set; }
        public object Parsed { get; set; }

        public override string ToString() => $"{Value}: {Representation}";

        public void Merge(TextInfo other)
        {
            throw new NotImplementedException();
        }

        static class ConstRep
        {
            private static int index = 0;
            private static char[] possibleChars = new char[]
            {
                '0',
                '1',
                '2',
                '3',
                '4',
                '5',
                '6',
                '7',
                '8',
                '9',
                'a',
                'b',
                'c',
                'd',
                'e',
                'f',
                'g',
                'h',
                'i',
                'j',
                'k',
                'l',
                'm',
                'n',
                'o',
                'p',
                'q',
                'r',
                's',
                't',
                'u',
                'v',
                'w',
                'x',
                'y',
                'z',
                'A',
                'B',
                'C',
                'D',
                'E',
                'F',
                'G',
                'H',
                'I',
                'J',
                'K',
                'L',
                'M',
                'N',
                'O',
                'P',
                'Q',
                'R',
                'S',
                'T',
                'U',
                'V',
                'W',
                'X',
                'Y',
                'Z',
            };
            public static string NextRep() => possibleChars[index++].ToString();
            //public static string NextRep(string regex) => NextRep();
            public static string NextRep(string regex) => Regex.Escape(regex);
        }

        // value types
        public static readonly string RawTextRepresentation = ConstRep.NextRep("<RawText>");
        public static readonly string ClassProperty = ConstRep.NextRep("<ClassProperty>");
        public static readonly string TextValue = ConstRep.NextRep("<Text>");
        public static readonly string BooleanValue = ConstRep.NextRep("<Bool>");
        public static readonly string NumberValue = ConstRep.NextRep("<Number>");
        public static readonly string DateValue = ConstRep.NextRep("<Date>");

        // grouping
        public static readonly string LogicalComparison = ConstRep.NextRep("<and_or>");
        public static readonly string ValueComparison = ConstRep.NextRep("<value_comparison>");
        public static readonly string BooleanFunction = ConstRep.NextRep("<func_bool>");
        public static readonly string IntFunction = ConstRep.NextRep("<func_int>");
        public static readonly string Add = ConstRep.NextRep("<+>");
        public static readonly string MathSymbols = ConstRep.NextRep("<+-/*>");
        public static readonly string Inverse = ConstRep.NextRep("<!>");

        // Expansion
        public static readonly string ExpandProperty = ConstRep.NextRep("<expandProperty>");
        public static readonly string ListOfExpands = ConstRep.NextRep("<ListOfExpand>");

        // text symbols
        public static readonly string ForwardSlash = ConstRep.NextRep("</>");
        public static readonly string Comma = ConstRep.NextRep("<,>");
        public static readonly string OpenParenthesis = ConstRep.NextRep("<open_grouping>");
        public static readonly string CloseParenthesis = ConstRep.NextRep("<close_grouping>");

        // sub-parameters
        public static readonly string SelectClause = ConstRep.NextRep("<select>");
        public static readonly string ExpandClause = ConstRep.NextRep("<expand>");
        public static readonly string FilterClause = ConstRep.NextRep("<filter>");

        public TextInfo() { }
        public TextInfo(Token token, Type type) : this(token) { Type = type; }
        public TextInfo(Token token)
        {
            Value = token.Value;
            Text = Value;
            if (!Enum.TryParse(token.Type, out TokenTypes type))
                Representation = RawTextRepresentation;
            else
            {
                switch (type)
                {
                    case TokenTypes.classProperties:
                        Representation = ClassProperty;
                        break;
                    case TokenTypes.forwardSlash:
                        Representation = ForwardSlash;
                        break;
                    case TokenTypes.comma:
                        Representation = Comma;
                        break;
                    case TokenTypes.not:
                        Representation = Inverse;
                        break;
                    case TokenTypes.greaterThan:
                    case TokenTypes.greaterThanOrEqual:
                    case TokenTypes.lessThan:
                    case TokenTypes.lessThanOrEqual:
                    case TokenTypes.equals:
                    case TokenTypes.notEquals:
                        Representation = ValueComparison;
                        break;
                    case TokenTypes.and:
                    case TokenTypes.or:
                        Representation = LogicalComparison;
                        break;
                    case TokenTypes.quotedString:
                        Representation = TextValue;
                        break;
                    case TokenTypes.filterClause:
                        Representation = FilterClause;
                        break;
                    case TokenTypes.selectClause:
                        Representation = SelectClause;
                        break;
                    case TokenTypes.expandClause:
                        Representation = ExpandClause;
                        break;
                    case TokenTypes.parenthesis:
                        if (Text == "(")
                            Representation = OpenParenthesis;
                        else if (Text == ")")
                            Representation = CloseParenthesis;
                        else
                            Representation = RawTextRepresentation;
                        break;
                    case TokenTypes.number:
                        if (Text.Contains("."))
                        {
                            Type = typeof(double);
                            Parsed = double.Parse(Text);
                        }
                        else
                        {
                            Type = typeof(long);
                            Parsed = long.Parse(Text);
                        }
                        Representation = NumberValue;
                        break;
                    default:
                        Representation = RawTextRepresentation;
                        break;
                }
            }
        }
    }
}
