using Immutability;
using Graph.Interfaces;
using NoData.Internal.TreeParser.Tokenizer;
using System;
using System.Linq;
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

        private static string GetRepresentationFromTokenType(TokenTypes type)
        {
            if (type == TokenTypes.classProperties)
                return TextRepresentation.ClassProperty;

            // value compare.
            if (new[] {
                        TokenTypes.greaterThan,
                        TokenTypes.greaterThanOrEqual,
                        TokenTypes.lessThan,
                        TokenTypes.lessThanOrEqual,
                        TokenTypes.equals,
                        TokenTypes.notEquals
                    }.Contains(type)
                )
                return TextRepresentation.ValueComparison;

            if (type == TokenTypes.not)
                return TextRepresentation.Inverse;

            // formatting
            if (type == TokenTypes.forwardSlash)
                return TextRepresentation.ForwardSlash;
            if (type == TokenTypes.semiColin)
                return TextRepresentation.SemiColin;
            if (type == TokenTypes.comma)
                return TextRepresentation.Comma;

            // logical compare
            if (type == TokenTypes.and || type == TokenTypes.or)
                return TextRepresentation.LogicalComparison;

            // const values
            if (type == TokenTypes.truth || type == TokenTypes.falsey)
                return TextRepresentation.LogicalComparison;
            if (type == TokenTypes.quotedString)
                return TextRepresentation.TextValue;

            // odata clauses.
            if (type == TokenTypes.filterClause)
                return TextRepresentation.FilterClause;
            if (type == TokenTypes.selectClause)
                return TextRepresentation.SelectClause;
            if (type == TokenTypes.expandClause)
                return TextRepresentation.ExpandClause;

            // sort direction
            if (type == TokenTypes.ascending || type == TokenTypes.descending)
                return TextRepresentation.SortOrder;
            if (type == TokenTypes.ascending || type == TokenTypes.descending)
                return TextRepresentation.SortOrder;

            // str functions
            if (type == TokenTypes.str_length)
                return TextRepresentation.StrLength;
            if (type == TokenTypes.str_ends_with)
                return TextRepresentation.StrEndsWith;
            if (type == TokenTypes.str_starts_with)
                return TextRepresentation.StrStartsWith;
            if (type == TokenTypes.str_index_of)
                return TextRepresentation.StrIndexOf;
            if (type == TokenTypes.str_contains)
                return TextRepresentation.StrContains;
            if (type == TokenTypes.str_replace)
                return TextRepresentation.StrReplace;
            if (type == TokenTypes.str_to_upper)
                return TextRepresentation.StrToUpper;
            if (type == TokenTypes.str_to_lower)
                return TextRepresentation.StrToLower;
            if (type == TokenTypes.str_trim)
                return TextRepresentation.StrTrim;
            if (type == TokenTypes.str_concat)
                return TextRepresentation.StrConcat;
            if (type == TokenTypes.str_substring)
                return TextRepresentation.StrSubString;

            throw new ArgumentException("Can't map the token type to a textual representation.");
        }

        public TextInfo(Token token)
        {
            Text = token.Value;
            if (!Enum.TryParse(token.Type, out TokenTypes type))
                Representation = TextRepresentation.RawTextRepresentation;
            else
            {
                if (type == TokenTypes.number)
                {
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
                }
                else if (type == TokenTypes.parenthesis)
                {
                    if (Text == "(")
                        Representation = TextRepresentation.OpenParenthesis;
                    else if (Text == ")")
                        Representation = TextRepresentation.CloseParenthesis;
                    else
                        Representation = TextRepresentation.RawTextRepresentation;
                }
                else
                    Representation = GetRepresentationFromTokenType(type);
            }
        }

        public override string ToString() => $"{Text}: {Representation}";
    }
}
