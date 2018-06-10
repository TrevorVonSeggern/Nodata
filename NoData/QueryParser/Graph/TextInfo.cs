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
#if DEBUG
            public static string NextRep(string regex) => Regex.Escape(regex);
#else
            public static string NextRep(string regex) => NextRep();
#endif
        }

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
        public static readonly string BooleanFunction = ConstRep.NextRep("<func_bool>");
        public static readonly string IntFunction = ConstRep.NextRep("<func_int>");
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
                    case TokenTypes.semiColin:
                        Representation = SemiColin;
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
                    case TokenTypes.truth:
                    case TokenTypes.falsey:
                        Representation = BooleanValue;
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
                    case TokenTypes.ascending:
                    case TokenTypes.descending:
                        Representation = SortOrder;
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
