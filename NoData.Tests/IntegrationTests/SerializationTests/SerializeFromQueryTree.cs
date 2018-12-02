using System.Collections.Generic;
using System.IO;
using System.Linq;
using AutoFixture;
using Bogus;
using FluentAssertions;
using NoData.Tests.SharedExampleClasses;
using Xunit;

namespace NoData.Tests.IntegrationTests.SerializationTests
{
    public class SerializeFromQueryTree
    {
        public static IEnumerable<Dto> FlatDtoEnumerable()
        {
            var regions = new[] { "en-US", "en-GB", "es-MX", "es-ES", "de" };
            var fixture = new Faker<Dto>()
                .RuleFor(x => x.id, f => f.IndexGlobal)
                .RuleFor(x => x.Name, f => f.Person.FirstName)
                .RuleFor(x => x.region_code, f => f.PickRandom(regions))
                .RuleFor(x => x.favorite, f => null)
                .RuleFor(x => x.partner, f => null)
                .RuleFor(x => x.children, f => new List<DtoChild>());

            return fixture.Generate(10);
        }

        [Fact]
        public void SerializeFromQueryTree_FromStream_ReturnsText()
        {
            // Given
            var query = FlatDtoEnumerable().ToList();
            var nodata = new NoDataBuilder<Dto>(new Parameters(), DefaultSettingsForType<Dto>.SettingsForType);

            // When
            var rawText = new StreamReader(nodata.Load(query.AsQueryable()).Stream()).ReadToEnd();

            // Then
            rawText.Should().NotBeNullOrWhiteSpace();
        }

        [Fact]
        public void SerializeFromQueryTree_FromStream_AndReserialized_SamePrimitiveTypes()
        {
            // Given
            var query = FlatDtoEnumerable().ToList();
            var nodata = new NoDataBuilder<Dto>(new Parameters(), DefaultSettingsForType<Dto>.SettingsForType);
            var rawText = new StreamReader(nodata.Load(query.AsQueryable()).Stream()).ReadToEnd();

            // When
            var result = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Dto>>(rawText);

            // Then
            result.Should().BeEquivalentTo(query);
        }

        [Fact]
        public void SerializeFromQueryTree_FromStream_AndReserialized_WithFavorite()
        {
            var regions = new[] { "en-US", "en-GB", "es-MX", "es-ES", "de" };
            // Given
            var favoriteFixture = new Faker<DtoChild>()
                .RuleFor(x => x.id, f => f.IndexGlobal)
                .RuleFor(x => x.Name, f => f.Person.FirstName)
                .RuleFor(x => x.region_code, f => f.PickRandom(regions))
                .RuleFor(x => x.partner, f => null)
                .RuleFor(x => x.favorite, f => null)
                .RuleFor(x => x.children, f => new List<DtoGrandChild>());
            var fixture = new Faker<Dto>()
                .RuleFor(x => x.id, f => f.IndexGlobal)
                .RuleFor(x => x.Name, f => f.Person.FirstName)
                .RuleFor(x => x.region_code, f => f.PickRandom(regions))
                .RuleFor(x => x.favorite, f => favoriteFixture.Generate())
                .RuleFor(x => x.partner, f => null)
                .RuleFor(x => x.children, f => new List<DtoChild>());

            var query = fixture.Generate(10).ToList();
            var nodata = new NoDataBuilder<Dto>(new Parameters("favorite"), DefaultSettingsForType<Dto>.SettingsForType);
            var rawText = new StreamReader(nodata.Load(query.AsQueryable()).Stream()).ReadToEnd();

            // When
            var result = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Dto>>(rawText);

            // Then
            result.Should().BeEquivalentTo(query);
        }

        [Fact]
        public void SerializeFromQueryTree_FromStream_AndReserialized_WithPartner()
        {
            var regions = new[] { "en-US", "en-GB", "es-MX", "es-ES", "de" };
            // Given
            var partnerFixture = new Faker<Dto>()
                .RuleFor(x => x.id, f => f.IndexGlobal)
                .RuleFor(x => x.Name, f => f.Person.FirstName)
                .RuleFor(x => x.region_code, f => f.PickRandom(regions))
                .RuleFor(x => x.partner, f => null)
                .RuleFor(x => x.favorite, f => null)
                .RuleFor(x => x.children, f => new List<DtoChild>());
            var fixture = new Faker<Dto>()
                .RuleFor(x => x.id, f => f.IndexGlobal)
                .RuleFor(x => x.Name, f => f.Person.FirstName)
                .RuleFor(x => x.region_code, f => f.PickRandom(regions))
                .RuleFor(x => x.partner, f => partnerFixture.Generate())
                .RuleFor(x => x.favorite, f => null)
                .RuleFor(x => x.children, f => new List<DtoChild>());

            var query = fixture.Generate(10).ToList();
            query.AddRange(query.Select(x => x.partner).ToList());
            var nodata = new NoDataBuilder<Dto>(new Parameters("partner"), DefaultSettingsForType<Dto>.SettingsForType);
            var rawText = new StreamReader(nodata.Load(query.AsQueryable()).Stream()).ReadToEnd();

            // When
            var result = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Dto>>(rawText);

            // Then
            result.Should().BeEquivalentTo(query);
        }

        [Fact]
        public void SerializeFromQueryTree_FromStream_AndReserialized_WithChildren()
        {
            var regions = new[] { "en-US", "en-GB", "es-MX", "es-ES", "de" };
            // Given
            var childFaker = new Faker<DtoChild>()
                .RuleFor(x => x.id, f => f.IndexGlobal)
                .RuleFor(x => x.Name, f => f.Person.FirstName)
                .RuleFor(x => x.region_code, f => f.PickRandom(regions))
                .RuleFor(x => x.partner, f => null)
                .RuleFor(x => x.favorite, f => null)
                .RuleFor(x => x.children, f => new List<DtoGrandChild>());
            var fixture = new Faker<Dto>()
                .RuleFor(x => x.id, f => f.IndexGlobal)
                .RuleFor(x => x.Name, f => f.Person.FirstName)
                .RuleFor(x => x.region_code, f => f.PickRandom(regions))
                .RuleFor(x => x.partner, f => null)
                .RuleFor(x => x.favorite, f => null)
                .RuleFor(x => x.children, f => childFaker.Generate(3));

            var query = fixture.Generate(10).ToList();
            var nodata = new NoDataBuilder<Dto>(new Parameters("children"), DefaultSettingsForType<Dto>.SettingsForType);
            var rawText = new StreamReader(nodata.Load(query.AsQueryable()).Stream()).ReadToEnd();

            // When
            var result = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Dto>>(rawText);

            // Then
            result.Should().BeEquivalentTo(query);
        }
    }
}