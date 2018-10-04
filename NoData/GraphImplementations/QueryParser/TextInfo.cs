using CodeTools;
using Graph.Interfaces;
using NoData.Internal.TreeParser.Tokenizer;
using System;
using System.Text.RegularExpressions;

namespace NoData.GraphImplementations.QueryParser
{
    [Immutable]
    public class TextInfo
    {
        public string Text { get; }
        public string Representation { get; }


        // Type and Parsed should be deleted
        public Type Type { get; }
        public object Parsed { get; }

        public TextInfo(string text, string representation)
        {
            Text = text;
            Representation = representation;
        }
        public TextInfo(string text, string representation, Type type) : this(text, representation)
        {
            Type = type;
        }

        public static TextInfo FromRepresentation(string representation) => new TextInfo(null, representation);
        public static TextInfo FromRepresentation(string representation, Type type) => new TextInfo(null, representation, type);

        public TextInfo(Token token)
        {
            Text = token.Value;
            if (!Enum.TryParse(token.Type, out TokenTypes type))
                Representation = TextRepresentation.RawTextRepresentation;
            else
            {
                switch (type)
                {
                    case TokenTypes.classProperties:
                        Representation = TextRepresentation.ClassProperty;
                        break;
                    case TokenTypes.forwardSlash:
                        Representation = TextRepresentation.ForwardSlash;
                        break;
                    case TokenTypes.semiColin:
                        Representation = TextRepresentation.SemiColin;
                        break;
                    case TokenTypes.comma:
                        Representation = TextRepresentation.Comma;
                        break;
                    case TokenTypes.not:
                        Representation = TextRepresentation.Inverse;
                        break;
                    case TokenTypes.greaterThan:
                    case TokenTypes.greaterThanOrEqual:
                    case TokenTypes.lessThan:
                    case TokenTypes.lessThanOrEqual:
                    case TokenTypes.equals:
                    case TokenTypes.notEquals:
                        Representation = TextRepresentation.ValueComparison;
                        break;
                    case TokenTypes.and:
                    case TokenTypes.or:
                        Representation = TextRepresentation.LogicalComparison;
                        break;
                    case TokenTypes.truth:
                    case TokenTypes.falsey:
                        Representation = TextRepresentation.BooleanValue;
                        break;
                    case TokenTypes.quotedString:
                        Representation = TextRepresentation.TextValue;
                        break;
                    case TokenTypes.filterClause:
                        Representation = TextRepresentation.FilterClause;
                        break;
                    case TokenTypes.selectClause:
                        Representation = TextRepresentation.SelectClause;
                        break;
                    case TokenTypes.expandClause:
                        Representation = TextRepresentation.ExpandClause;
                        break;
                    case TokenTypes.ascending:
                    case TokenTypes.descending:
                        Representation = TextRepresentation.SortOrder;
                        break;
                    case TokenTypes.parenthesis:
                        if (Text == "(")
                            Representation = TextRepresentation.OpenParenthesis;
                        else if (Text == ")")
                            Representation = TextRepresentation.CloseParenthesis;
                        else
                            Representation = TextRepresentation.RawTextRepresentation;
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
                        Representation = TextRepresentation.NumberValue;
                        break;
                    default:
                        Representation = TextRepresentation.RawTextRepresentation;
                        break;
                }
            }
        }

        public override string ToString() => $"{Text}: {Representation}";
    }
}
