using System.Collections.Generic;
using Xunit;
using NoData.Internal.TreeParser.Tokenizer;
using System;

namespace NoData.Tests.TokenizerTests
{
    public class TokenizerTest
    {
        static readonly string[] abcProperties = new string[] { "a", "b", "c" };

        [Fact]
        public void TestABC()
        {
            var izer = new Tokenizer(abcProperties);

            var result = new List<Token>();
            result.AddRange(izer.Tokenize("a b c"));

            Assert.Equal(3, result.Count);
            Assert.Equal("a", result[0].Value);
            Assert.Equal("b", result[1].Value);
            Assert.Equal("c", result[2].Value);
        }

        [Fact]
        public void TestABC_WithLargeSpaces()
        {
            var izer = new Tokenizer(abcProperties);

            var result = new List<Token>();
            result.AddRange(izer.Tokenize("a       b   c"));

            Assert.Equal(3, result.Count);
            Assert.Equal("a", result[0].Value);
            Assert.Equal("b", result[1].Value);
            Assert.Equal("c", result[2].Value);
        }

        [Fact]
        public void TestABC_Expression()
        {
            var izer = new Tokenizer(abcProperties);

            var result = new List<Token>();
            result.AddRange(izer.Tokenize("a+b-c"));

            Assert.Equal(5, result.Count);
            int i = 0;
            Assert.Equal("a", result[i++].Value);
            Assert.Equal("+", result[i++].Value);
            Assert.Equal("b", result[i++].Value);
            Assert.Equal("-", result[i++].Value);
            Assert.Equal("c", result[i++].Value);
        }

        [Fact]
        public void TestABC_EqualsWithSpace_Success()
        {
            var izer = new Tokenizer(abcProperties);

            var result = new List<Token>();
            result.AddRange(izer.Tokenize("a + b ne c"));

            Assert.Equal(5, result.Count);
            int i = 0;
            Assert.Equal("a", result[i++].Value);
            Assert.Equal("+", result[i++].Value);
            Assert.Equal("b", result[i++].Value);
            Assert.Equal("ne", result[i++].Value);
            Assert.Equal("c", result[i++].Value);
        }

        [Fact]
        public void TestABC_EqualsWithNoSpace_Fails()
        {
            var izer = new Tokenizer(abcProperties);

            var result = new List<Token>();
            Assert.ThrowsAny<Exception>(() =>
            {
                result.AddRange(izer.Tokenize("a+beqc"));
            });
        }

        [Fact]
        public void TestABC_Expression_WithSpaces()
        {
            var izer = new Tokenizer(abcProperties);

            var result = new List<Token>();
            result.AddRange(izer.Tokenize("a + b ne c"));

            Assert.Equal(5, result.Count);
            int i = 0;
            Assert.Equal("a", result[i++].Value);
            Assert.Equal("+", result[i++].Value);
            Assert.Equal("b", result[i++].Value);
            Assert.Equal("ne", result[i++].Value);
            Assert.Equal("c", result[i++].Value);
        }

        [Fact]
        public void TestABC_Expression_WithGroups()
        {
            var izer = new Tokenizer(abcProperties);

            var result = new List<Token>();
            result.AddRange(izer.Tokenize("a and (b ne c)"));

            var expected = new string[]
            {
                "a", "and", "(", "b", "ne", "c", ")"
            };

            Assert.Equal(result.Count, expected.Length);
            for (int i = 0; i < expected.Length; ++i)
            {
                Assert.Equal(expected[i], result[i].Value);
            }
        }

        [Fact]
        public void TestABC_GreaterThan1_Success()
        {
            var izer = new Tokenizer(abcProperties);

            var result = new List<Token>();
            result.AddRange(izer.Tokenize("a gt 1"));

            var expected = new string[]
            {
                "a", "gt", "1"
            };

            Assert.Equal(result.Count, expected.Length);
            for (int i = 0; i < expected.Length; ++i)
            {
                Assert.Equal(result[i].Value, expected[i]);
            }
        }
    }
}
