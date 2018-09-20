using Xunit;
using System;
using NoData.Internal.TreeParser.Tokenizer;

namespace NoData.Tests.TokenizerTests
{
    public class TokenTest
    {
        [Theory]
        [InlineData("token")]
        public void Test1(string data)
        {
            var t = new Token(data);
            Assert.Equal(data, t.Value);
        }
    }
}
