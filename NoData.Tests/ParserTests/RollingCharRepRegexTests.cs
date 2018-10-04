using Xunit;
using FluentAssertions;
using NoData.GraphImplementations.QueryParser;

namespace NoData.Tests.ParserTests
{
    public class RollingCharRepRegexTests
    {
        RollingCharacterRegexRepresentation roller = new RollingCharacterRegexRepresentation();

        [Fact]
        public void RollingCharRepRegex_FirstAreSingleCharacter()
        {
            //When
            var charString = roller.NextRep();

            //Then
            charString.Should().Be("<0");
        }

        [Fact]
        public void RollingCharRepRegex_CanGetLargerNumbers()
        {
            for (int i = 0; i < 62; ++i)
                roller.NextRep().Should().MatchRegex(@"<[a-zA-Z0-9]");
            for (int i = 0; i < 62; ++i)
                roller.NextRep().Should().MatchRegex(@"<0[a-zA-Z0-9]");
            for (int i = 0; i < 62; ++i)
                roller.NextRep().Should().MatchRegex(@"<1[a-zA-Z0-9]");
        }
    }
}