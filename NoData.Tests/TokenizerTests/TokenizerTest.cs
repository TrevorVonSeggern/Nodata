using System.Collections.Generic;
using NUnit.Framework;
using NoData.Internal.TreeParser.Tokenizer;

namespace NoData.Tests.TokenizerTests
{
    [TestFixture]
    public class TokenizerTest
    {
        static readonly string[] abcProperties = new string[] { "a", "b", "c" };

        [Test]
        public void TestABC()
        {
            var izer = new Tokenizer(abcProperties);

            var result = new List<Token>();
            result.AddRange(izer.Tokenize("a b c"));

            Assert.AreEqual(result.Count, 3);
            Assert.AreEqual(result[0].Value, "a");
            Assert.AreEqual(result[1].Value, "b");
            Assert.AreEqual(result[2].Value, "c");
        }

        [Test]
        public void TestABC_WithLargeSpaces()
        {
            var izer = new Tokenizer(abcProperties);

            var result = new List<Token>();
            result.AddRange(izer.Tokenize("a       b   c"));

            Assert.AreEqual(result.Count, 3);
            Assert.AreEqual(result[0].Value, "a");
            Assert.AreEqual(result[1].Value, "b");
            Assert.AreEqual(result[2].Value, "c");
        }

        [Test]
        public void TestABC_Expression()
        {
            var izer = new Tokenizer(abcProperties);

            var result = new List<Token>();
            result.AddRange(izer.Tokenize("a+b-c"));

            Assert.AreEqual(result.Count, 5);
            int i = 0;
            Assert.AreEqual(result[i++].Value, "a");
            Assert.AreEqual(result[i++].Value, "+");
            Assert.AreEqual(result[i++].Value, "b");
            Assert.AreEqual(result[i++].Value, "-");
            Assert.AreEqual(result[i++].Value, "c");
        }

        [Test]
        public void TestABC_EqualsWithSpace_Success()
        {
            var izer = new Tokenizer(abcProperties);

            var result = new List<Token>();
            result.AddRange(izer.Tokenize("a + b ne c"));

            Assert.AreEqual(result.Count, 5);
            int i = 0;
            Assert.AreEqual(result[i++].Value, "a");
            Assert.AreEqual(result[i++].Value, "+");
            Assert.AreEqual(result[i++].Value, "b");
            Assert.AreEqual(result[i++].Value, "ne");
            Assert.AreEqual(result[i++].Value, "c");
        }

        [Test]
        public void TestABC_EqualsWithNoSpace_Fails()
        {
            var izer = new Tokenizer(abcProperties);

            var result = new List<Token>();
            Assert.That(() =>
            {
                result.AddRange(izer.Tokenize("a+beqc"));
            }, Throws.Exception);
        }

        [Test]
        public void TestABC_Expression_WithSpaces()
        {
            var izer = new Tokenizer(abcProperties);

            var result = new List<Token>();
            result.AddRange(izer.Tokenize("a + b ne c"));

            Assert.AreEqual(result.Count, 5);
            int i = 0;
            Assert.AreEqual(result[i++].Value, "a");
            Assert.AreEqual(result[i++].Value, "+");
            Assert.AreEqual(result[i++].Value, "b");
            Assert.AreEqual(result[i++].Value, "ne");
            Assert.AreEqual(result[i++].Value, "c");
        }

        [Test]
        public void TestABC_Expression_WithGroups()
        {
            var izer = new Tokenizer(abcProperties);

            var result = new List<Token>();
            result.AddRange(izer.Tokenize("a and (b ne c)"));

            var expected = new string[]
            {
                "a", "and", "(", "b", "ne", "c", ")"
            };

            Assert.AreEqual(result.Count, expected.Length);
            for(int i = 0; i < expected.Length; ++i)
            {
                Assert.AreEqual(result[i].Value, expected[i]);
            }
        }

        [Test]
        public void TestABC_GreaterThan1_Success()
        {
            var izer = new Tokenizer(abcProperties);

            var result = new List<Token>();
            result.AddRange(izer.Tokenize("a gt 1"));

            var expected = new string[]
            {
                "a", "gt", "1"
            };

            Assert.AreEqual(result.Count, expected.Length);
            for(int i = 0; i < expected.Length; ++i)
            {
                Assert.AreEqual(result[i].Value, expected[i]);
            }
        }
    }
}
