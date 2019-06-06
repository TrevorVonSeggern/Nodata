using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using SampleWebApi.Models;
using Xunit;

namespace NoData.Tests.IntegrationTests.EndToEnd.SampleWebApiTests
{
    public class ApiTests
    {
        public HttpClient Client { get; private set; }

        public ApiTests()
        {
            var server = new TestServer(new WebHostBuilder().UseStartup<SampleWebApi.Startup>());

            Client = server.CreateClient();
        }

        [Fact]
        public async Task ApiTests_Get_CorrectStatusCode()
        {
            // Given
            var response = await Client.GetAsync("/api/values");

            // When
            response.EnsureSuccessStatusCode();

            // Then
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task ApiTests_Get_Response_CanDeserialize()
        {
            // Given
            var response = await Client.GetAsync("/api/values");

            // When
            response.EnsureSuccessStatusCode();
            var result = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Dto>>(await response.Content.ReadAsStringAsync());

            // Then
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            result.Should().NotBeNullOrEmpty();
            foreach (var dto in result)
            {
                dto.Should().NotBeNull();
                dto.id.Should().BeInRange(1, 6);
                dto.partner.Should().BeNull();
                dto.favorite.Should().BeNull();
                dto.children.Should().BeNullOrEmpty();
            }
        }
    }
}