using Immutability;
using NoData.Internal.TreeParser.Tokenizer;
using System;
using System.Globalization;
using System.Linq;

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

        private static string GetRepresentationFromTokenType(TokenType type)
        {
            if (type == TokenType.classProperties)
                return TextRepresentation.ClassProperty;

            // value compare.
            if (new[] {
                        TokenType.greaterThan,
                        TokenType.greaterThanOrEqual,
                        TokenType.lessThan,
                        TokenType.lessThanOrEqual,
                        TokenType.equals,
                        TokenType.notEquals
                    }.Contains(type)
                )
                return TextRepresentation.ValueComparison;

            if (type == TokenType.not)
                return TextRepresentation.Inverse;

            // formatting
            if (type == TokenType.forwardSlash)
                return TextRepresentation.ForwardSlash;
            if (type == TokenType.semiColin)
                return TextRepresentation.SemiColin;
            if (type == TokenType.comma)
                return TextRepresentation.Comma;

            // logical compare
            if (type == TokenType.and || type == TokenType.or)
                return TextRepresentation.LogicalComparison;

            // const values
            if (type == TokenType.truth || type == TokenType.falsey)
                return TextRepresentation.LogicalComparison;
            if (type == TokenType.quotedString)
                return TextRepresentation.TextValue;

            // odata clauses.
            if (type == TokenType.filterClause)
                return TextRepresentation.FilterClause;
            if (type == TokenType.selectClause)
                return TextRepresentation.SelectClause;
            if (type == TokenType.expandClause)
                return TextRepresentation.ExpandClause;

            // sort direction
            if (type == TokenType.ascending || type == TokenType.descending)
                return TextRepresentation.SortOrder;
            if (type == TokenType.ascending || type == TokenType.descending)
                return TextRepresentation.SortOrder;

            // str functions
            if (type == TokenType.strLength)
                return TextRepresentation.StrLength;
            if (type == TokenType.strEndsWith)
                return TextRepresentation.StrEndsWith;
            if (type == TokenType.strStartsWith)
                return TextRepresentation.StrStartsWith;
            if (type == TokenType.strIndexOf)
                return TextRepresentation.StrIndexOf;
            if (type == TokenType.strContains)
                return TextRepresentation.StrContains;
            if (type == TokenType.strReplace)
                return TextRepresentation.StrReplace;
            if (type == TokenType.strToUpper)
                return TextRepresentation.StrToUpper;
            if (type == TokenType.strToLower)
                return TextRepresentation.StrToLower;
            if (type == TokenType.strTrim)
                return TextRepresentation.StrTrim;
            if (type == TokenType.strConcat)
                return TextRepresentation.StrConcat;
            if (type == TokenType.strSubstring)
                return TextRepresentation.StrSubString;

            throw new ArgumentException("Can't map the token type to a textual representation.");
        }

        public TextInfo(Token token)
        {
            Text = token.Value;
            if (!Enum.TryParse(token.Type, out TokenType type))
                Representation = TextRepresentation.RawTextRepresentation;
            else
            {
                if (type == TokenType.number)
                {
                    if (long.TryParse(Text, NumberStyles.Integer, CultureInfo.CurrentCulture.NumberFormat, out var longParsed))
                    {
                        Type = typeof(long);
                        Parsed = longParsed;
                    }
                    else
                    {
                        Type = typeof(double);
                        Parsed = double.Parse(Text, CultureInfo.CurrentCulture.NumberFormat);
                    }
                    Representation = TextRepresentation.NumberValue;
                }
                else if (type == TokenType.parenthesis)
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
