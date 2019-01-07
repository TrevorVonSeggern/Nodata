using System.Collections.Generic;
using System.Linq;
using Xunit;
using NoData.Utility;
using System.IO;
using FluentAssertions;
using System.Text;

namespace NoData.Tests.UtilityTests
{
    public class StreamOfStreamsTest
    {
        public StreamOfStreamsTest()
        {
        }

        Stream CreateStreamFromString(string contents)
        {
            byte[] buffer = Encoding.ASCII.GetBytes(contents);
            return new MemoryStream(buffer);
        }

        string ReadStream(Stream stream)
        {
            var beforePosition = stream.Position;
            string result = "";
            using (var sr = new StreamReader(stream))
            {
                result = sr.ReadToEnd();
            }
            return result;
        }

        [Fact]
        public void HelperMethods_WorkAsExpected()
        {
            // Given
            var str = "content from one string.";
            var source = CreateStreamFromString(str);

            // When
            var result = ReadStream(source);

            // Then
            result.Should().Be(str);
        }

        [Fact]
        public void OneMemoryStream_OneStreamWithContents()
        {
            // Given
            var str = "content from one string.";
            var source = CreateStreamFromString(str);

            // When
            Stream combined = new StreamOfStreams(new[] { source });
            var result = ReadStream(combined);

            // Then
            result.Should().Be(str);
        }

        [Fact]
        public void OneMemoryStream_TwoStreamsOfContent()
        {
            // Given
            var A_str = "this is from 'A'";
            var B_str = "Yo this is from b dog";
            var sourceA = CreateStreamFromString(A_str);
            var sourceB = CreateStreamFromString(B_str);

            // When
            Stream combined = new StreamOfStreams(new[] { sourceA, sourceB });
            var result = ReadStream(combined);

            // Then
            result.Should().Be(A_str + B_str);
        }

        [Fact]
        public void OneMemoryStream_ThreeStreamsOfContent()
        {
            // Given
            var A_str = "this is from 'A'.";
            var B_str = "Yo this is from b dog.";
            var C_str = "If I may interject, this is C stream.";
            var sourceA = CreateStreamFromString(A_str);
            var sourceB = CreateStreamFromString(B_str);
            var sourceC = CreateStreamFromString(C_str);

            // When
            Stream combined = new StreamOfStreams(new[] { sourceA, sourceB, sourceC });
            var result = ReadStream(combined);

            // Then
            result.Should().Be(A_str + B_str + C_str);
        }

        [Fact]
        public void OneMemoryStream_TwoStreamsOfContentOneEmpty()
        {
            // Given
            var A_str = "this is from 'A'.";
            var C_str = "If I may interject, this is C stream.";
            var sourceA = CreateStreamFromString(A_str);
            var sourceB = CreateStreamFromString("");
            var sourceC = CreateStreamFromString(C_str);

            // When
            Stream combined = new StreamOfStreams(new[] { sourceA, sourceB, sourceC });
            var result = ReadStream(combined);

            // Then
            result.Should().Be(A_str + C_str);
        }
    }
}
