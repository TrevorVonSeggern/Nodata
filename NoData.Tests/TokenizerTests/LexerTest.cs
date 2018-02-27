using System.Collections.Generic;
using NUnit.Framework;
using NoData.Internal.TreeParser.Tokenizer;

namespace TokenizerTests
{
    [TestFixture]
    public class LexerTest
    {
        [Test]
        public void TestABC()
        {
            var lexer = new Lexer();
            lexer.AddDefinition(new TokenDefinition("a", TokenTypes.classProperties));
            lexer.AddDefinition(new TokenDefinition("b", TokenTypes.classProperties));
            lexer.AddDefinition(new TokenDefinition("c", TokenTypes.classProperties));
            lexer.AddDefinition(new TokenDefinition(@"\s+", TokenTypes.whitespace));

            var result = new List<Token>();
            result.AddRange(lexer.Tokenize("abc"));

            Assert.AreEqual(result.Count, 3);
            Assert.AreEqual(result[0].Value, "a");
            Assert.AreEqual(result[1].Value, "b");
            Assert.AreEqual(result[2].Value, "c");
        }

        [Test]
        public void TestABC_WithSpaces()
        {
            var lexer = new Lexer();
            lexer.AddDefinition(new TokenDefinition("a", TokenTypes.classProperties));
            lexer.AddDefinition(new TokenDefinition("b", TokenTypes.classProperties));
            lexer.AddDefinition(new TokenDefinition("c", TokenTypes.classProperties));
            lexer.AddDefinition(new TokenDefinition(@"\s+", TokenTypes.whitespace));

            var result = new List<Token>();
            result.AddRange(lexer.Tokenize("a  b   c"));

            Assert.AreEqual(result.Count, 5);
            Assert.AreEqual(result[0].Value, "a");
            Assert.AreEqual(result[1].Value.Trim(), "");
            Assert.AreEqual(result[2].Value, "b");
            Assert.AreEqual(result[3].Value.Trim(), "");
            Assert.AreEqual(result[4].Value, "c");
        }
    }
}
