using NUnit.Framework;
using System;
using NoData.Internal.TreeParser.Tokenizer;

namespace TokenizerTests
{
    [TestFixture]
    public class TokenTest
    {
        [Test]
        [TestCase("token")]
        public void Test1(string data)
        {
            var t = new Token(data);
            Assert.AreEqual(t.Value, data);
        }
    }
}
