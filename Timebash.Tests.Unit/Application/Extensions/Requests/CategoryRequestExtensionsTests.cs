using Bogus;
using FluentAssertions;
using Timebash.Application.Extensions.Requests;
using Timebash.Core.DTOs.Requests;

namespace Timebash.Tests.Unit.Application.Extensions.Requests;

public class CategoryRequestExtensionsTests
{
    private static readonly Faker _faker = new();

    [Fact]
    public void ToCategory_ShouldMapAllPropertiesCorrectly()
    {
        var id = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var request = new CategoryRequest(_faker.Lorem.Word(), "#000000", [.. _faker.Lorem.Words()]);

        var result = request.ToCategory(id, userId);

        result.Id.Should().Be(id);
        result.UserId.Should().Be(userId);
        result.Name.Should().Be(request.Name);
        result.Color.Should().Be(request.Color);
        result.Keywords.Should().BeEquivalentTo(request.Keywords);
    }

    [Fact]
    public void ToCategory_WithNullKeywords_ShouldMapEmptyList()
    {
        var id = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var request = new CategoryRequest(_faker.Lorem.Word(), "#000000", null);

        var result = request.ToCategory(id, userId);
        result.Keywords.Should().BeEmpty();
    }
}
