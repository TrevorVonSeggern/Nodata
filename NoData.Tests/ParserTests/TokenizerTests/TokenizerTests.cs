using System.Collections.Generic;
using Xunit;
using NoData.Internal.TreeParser.Tokenizer;
using System;
using NoData.Tests.SharedExampleClasses;
using System.Linq;
using FluentAssertions;

namespace NoData.Tests.ParserTests.TokenizerTests
{
    public class TokenizerTests
    {
        static readonly string[] classProperties = GraphImplementations.Schema.GraphSchema.Cache<Dto>.Graph
                                                    .VertexContainingType(typeof(Dto)).Value.Properties
                                                    .Select(x => x.Name).ToArray();

        static readonly string[] abcProperties = new string[] { "a", "b", "c" };

        [Theory]
        [InlineData("token")]
        [InlineData("tokenadsfasdf")]
        [InlineData("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa")]
        [InlineData("a")]
        [InlineData("_this_is_a_test_")]
        public void Test1(string data)
        {
            var t = new Token(data);
            Assert.Equal(data, t.Value);
        }

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

        [Theory]
        [InlineData("Name")]
        [InlineData("id")]
        [InlineData("region_code")]
        [InlineData("favorite")]
        [InlineData("partner")]
        [InlineData("children")]
        public void Tokenize_WithClass_Properties(string propertyName)
        {
            var izer = new Tokenizer(classProperties);

            var result = new List<Token>();
            result.AddRange(izer.Tokenize(propertyName + " gt 1"));

            var expected = new string[]
            {
                propertyName, "gt", "1"
            };


            Assert.Equal(result.Count, expected.Length);
            for (int i = 0; i < expected.Length; ++i)
            {
                Assert.Equal(result[i].Value, expected[i]);
            }
        }

        [Theory]
        [InlineData("length(Name) eq 1")]
        [InlineData("concat('Georg', 'e') eq 'George'")]
        [InlineData("substring('George', 1) eq 'G'")]
        [InlineData("substring('George', 0, 1) eq 'G'")]
        [InlineData("trim(' George ') eq Name")]
        [InlineData("tolower('GeOrGe ' eq 'george'")]
        [InlineData("toupper('GeOrGe ' eq 'GEORGE'")]
        [InlineData("contains(Name,'eorge')")]
        [InlineData("endswith(Name,'eorge')")]
        [InlineData("startswith(Name,'ge')")]
        [InlineData("replace(Name, 'Name', 'ReplacedName') eq 'ReplacedName'")]
        [InlineData("indexof(Name, 'eorge') eq 1")]
        public void Tokenize_WithNameProperties_And_StringFilters(string toParse)
        {
            var izer = new Tokenizer(classProperties);

            var result = new List<Token>();
            result.AddRange(izer.Tokenize(toParse));
            result.Should().NotBeNullOrEmpty();
        }
    }
}
