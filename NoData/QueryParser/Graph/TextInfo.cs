using NoData.Graph.Interfaces;
using NoData.Internal.TreeParser.Tokenizer;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
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

        // value types
        public static readonly string RawTextRepresentation = Regex.Escape("<RawText>");
        public static readonly string ClassProperty = Regex.Escape("<ClassProperty>");
        public static readonly string TextValue = Regex.Escape("<Text>");
        public static readonly string BooleanValue = Regex.Escape("<Bool>");
        public static readonly string NumberValue = Regex.Escape("<Number>");
        public static readonly string DateValue = Regex.Escape("<Date>");

        // grouping
        public static readonly string LogicalComparison = Regex.Escape("<and_or>");
        public static readonly string ValueComparison = Regex.Escape("<value_comparison>");
        public static readonly string BooleanFunction = Regex.Escape("<func_bool>");
        public static readonly string IntFunction = Regex.Escape("<func_int>");
        public static readonly string Add = Regex.Escape("<+>");
        public static readonly string MathSymbols = Regex.Escape("<+-/*>");
        public static readonly string Inverse = Regex.Escape("<!>");

        // Expansion
        public static readonly string ExpandProperty = Regex.Escape("<expandProperty>");
        public static readonly string ListOfExpands = Regex.Escape("<ListOfExpand>");

        // text symbols
        public static readonly string ForwardSlash = Regex.Escape("</>");
        public static readonly string Comma = Regex.Escape("<,>");
        public static readonly string OpenParenthesis = Regex.Escape("<open_grouping>");
        public static readonly string CloseParenthesis = Regex.Escape("<close_grouping>");

        // sub-parameters
        public static readonly string SelectClause = Regex.Escape("<select>");
        public static readonly string ExpandClause = Regex.Escape("<expand>");
        public static readonly string FilterClause = Regex.Escape("<filter>");

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
