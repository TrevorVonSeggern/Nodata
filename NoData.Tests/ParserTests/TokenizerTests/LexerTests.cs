using System.Collections.Generic;
using Xunit;
using NoData.Internal.TreeParser.Tokenizer;

namespace NoData.Tests.ParserTests.TokenizerTests
{
    public class LexerTests
    {
        [Fact]
        public void TestABC()
        {
            var lexer = new Lexer();
            lexer.AddDefinition(new TokenDefinition("a", TokenType.classProperties));
            lexer.AddDefinition(new TokenDefinition("b", TokenType.classProperties));
            lexer.AddDefinition(new TokenDefinition("c", TokenType.classProperties));
            lexer.AddDefinition(new TokenDefinition(@"\s+", TokenType.whitespace));

            var result = new List<Token>();
            result.AddRange(lexer.Tokenize("abc"));

            Assert.Equal(3, result.Count);
            Assert.Equal("a", result[0].Value);
            Assert.Equal("b", result[1].Value);
            Assert.Equal("c", result[2].Value);
        }

        [Fact]
        public void TestABC_WithSpaces()
        {
            var lexer = new Lexer();
            lexer.AddDefinition(new TokenDefinition("a", TokenType.classProperties));
            lexer.AddDefinition(new TokenDefinition("b", TokenType.classProperties));
            lexer.AddDefinition(new TokenDefinition("c", TokenType.classProperties));
            lexer.AddDefinition(new TokenDefinition(@"\s+", TokenType.whitespace));

            var result = new List<Token>();
            result.AddRange(lexer.Tokenize("a  b   c"));

            Assert.Equal(5, result.Count);
            Assert.Equal("a", result[0].Value);
            Assert.Equal("", result[1].Value.Trim());
            Assert.Equal("b", result[2].Value);
            Assert.Equal("", result[3].Value.Trim());
            Assert.Equal("c", result[4].Value);
        }
    }
}
