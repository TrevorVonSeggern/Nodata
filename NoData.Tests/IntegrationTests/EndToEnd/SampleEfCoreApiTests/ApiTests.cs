using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Bogus;
using FluentAssertions;
using Graph;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using SampleEFCoreApi.Models;
using Xunit;

namespace NoData.Tests.IntegrationTests.EndToEnd.SampleEfCoreApiTests
{
    public class ApiTests
    {
        public HttpClient Client { get; private set; }

        private DtoPersonModify ModifyPerson(DtoPerson person)
        {
            return new DtoPersonModify()
            {
                Name = person.Name,
                Region_code = person.Region_code,
                PartnerId = person.PartnerId,
                FavoriteId = person.FavoriteId,
            };
        }

        private Faker<DtoPersonCreate> FakePerson()
        {
            var regions = new[] { "en-US", "en-GB", "es-MX", "es-ES", "de" };
            return new Faker<DtoPersonCreate>()
                .RuleFor(x => x.Name, f => f.Person.FirstName)
                .RuleFor(x => x.Region_code, f => f.PickRandom(regions))
                .RuleFor(x => x.FavoriteId, f => null)
                .RuleFor(x => x.PartnerId, f => null)
                ;
        }

        private Faker<DtoChildCreate> FakeChild()
        {
            var regions = new[] { "en-US", "en-GB", "es-MX", "es-ES", "de" };
            return new Faker<DtoChildCreate>()
                .RuleFor(x => x.Name, f => f.Person.FirstName)
                .RuleFor(x => x.Region_code, f => f.PickRandom(regions))
                .RuleFor(x => x.FavoriteId, f => null)
                .RuleFor(x => x.PartnerId, f => null)
                ;
        }

        private async Task<List<TF>> Post<T, TF>(Faker<T> faker, int count, string url)
            where T : class
        {
            var toCreate = faker.Generate(count);

            var posted = new List<TF>();
            foreach (var dto in toCreate)
            {
                var postResponse = await Client.PostAsync(
                    url,
                    new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(dto),
                    Encoding.UTF8,
                    "application/json"
                ));
                postResponse.EnsureSuccessStatusCode();
                Assert.Equal(HttpStatusCode.Created, postResponse.StatusCode);
                var result = Newtonsoft.Json.JsonConvert.DeserializeObject<TF>(await postResponse.Content.ReadAsStringAsync());
                posted.Add(result);
            }
            return posted;
        }

        private async Task<List<T>> Patch<T>(IEnumerable<ITuple<int, T>> toModify, string urlBase)
            where T : class
        {
            var result = new List<T>();
            foreach (var tuple in toModify)
            {
                var dto = tuple.Item2;
                var postResponse = await Client.SendAsync(new HttpRequestMessage(
                    new HttpMethod("PATCH"),
                    urlBase + "/" + tuple.Item1)
                {
                    Content = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(dto),
                        Encoding.UTF8,
                        "application/json")
                });
                postResponse.EnsureSuccessStatusCode();
                Assert.Equal(HttpStatusCode.NoContent, postResponse.StatusCode);
                result.Add(dto);
            }
            return result;
        }

        private Task<List<DtoPersonModify>> PatchPeople(IEnumerable<ITuple<int, DtoPersonModify>> people)
        {
            return Patch(people, "/api/people");
        }
        private Task<List<DtoPerson>> PostPeople(Faker<DtoPersonCreate> faker, int count)
        {
            return Post<DtoPersonCreate, DtoPerson>(faker, count, "/api/people");
        }

        private Task<List<DtoChild>> PostChildren(Faker<DtoChildCreate> faker, int count)
        {
            return Post<DtoChildCreate, DtoChild>(faker, count, "/api/child");
        }

        private async Task<List<DtoPerson>> GetPeople_OK(string expand = null)
        {
            var url = "/api/people";
            if (!string.IsNullOrWhiteSpace(expand))
                url += "?$expand=" + expand;
            var response = await Client.GetAsync(url);

            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var str = await response.Content.ReadAsStringAsync();
            var result = Newtonsoft.Json.JsonConvert.DeserializeObject<List<DtoPerson>>(str);

            return result;
        }

        public ApiTests()
        {
            var server = new TestServer(new WebHostBuilder().UseStartup<SampleEFCoreApi.Startup>());

            Client = server.CreateClient();
        }

        [Fact]
        public async Task ApiTests_Get_CorrectStatusCode()
        {
            // Given
            var response = await Client.GetAsync("/api/people");

            // When
            response.EnsureSuccessStatusCode();

            // Then
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task ApiTests_Get_People_FirstGetIsEmpty_Success()
        {
            var result = await GetPeople_OK();
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task ApiTests_PostThenGet_People_EntityReturned()
        {
            var count = 10;
            var postedPeople = await PostPeople(FakePerson(), count);

            var result = await GetPeople_OK();

            // Then
            result.Should().NotBeNullOrEmpty();
            result.Should().HaveCount(count);

            foreach (var dto in result)
            {
                dto.Should().NotBeNull();
                dto.Id.Should().BeGreaterThan(0);
                dto.Name.Should().BeOneOf(postedPeople.Select(x => x.Name));
                dto.Region_code.Should().BeOneOf(postedPeople.Select(x => x.Region_code));
                dto.Partner.Should().BeNull();
                dto.Favorite.Should().BeNull();
                dto.Children.Should().BeNullOrEmpty();
            }
        }

        [Fact]
        public async Task ApiTests_PostWithChildren_People_EntityReturned()
        {
            var count = 1;
            var peopleFake = FakePerson();
            var postedPeople = await PostPeople(peopleFake, count);
            var postedChildren = await PostChildren(FakeChild(), count);
            var toPatch = postedPeople.Select(p =>
            {
                var toModify = ModifyPerson(p);
                toModify.FavoriteId = postedChildren.Select(x => x.Id).First();
                return ITuple.Create(p.Id, toModify);
            }).ToList();
            await PatchPeople(toPatch);
            var result = await GetPeople_OK("Favorite");

            // Then
            result.Should().NotBeNullOrEmpty();
            result.Should().HaveCount(count);

            foreach (var dto in result)
            {
                dto.Should().NotBeNull();
                dto.Id.Should().BeGreaterThan(0);
                dto.Name.Should().BeOneOf(postedPeople.Select(x => x.Name));
                dto.Region_code.Should().BeOneOf(postedPeople.Select(x => x.Region_code));
                dto.Partner.Should().BeNull();
                dto.Favorite.Should().NotBeNull();
                dto.Children.Should().BeNullOrEmpty();
            }
        }

        [Fact]
        public async Task ApiTests_Get_People_GetIsStillEmpty_Success()
        {
            // Given
            var response = await Client.GetAsync("/api/people");

            // When
            response.EnsureSuccessStatusCode();
            var result = Newtonsoft.Json.JsonConvert.DeserializeObject<List<DtoPerson>>(await response.Content.ReadAsStringAsync());

            // Then
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            result.Should().BeEmpty();
            foreach (var dto in result)
            {
                dto.Should().NotBeNull();
                dto.Id.Should().BeInRange(1, 6);
                dto.Partner.Should().BeNull();
                dto.Favorite.Should().BeNull();
                dto.Children.Should().BeNullOrEmpty();
            }
        }
    }
}