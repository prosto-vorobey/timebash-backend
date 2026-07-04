using Bogus;
using Timebash.Core.Entities;
using Timebash.Application.Extensions;
using FluentAssertions;
using Timebash.Core.DTOs.Requests;

namespace Timebash.Tests.Unit.Application.Extensions;

public class CategoryExtensionsTests
{
    private static readonly Faker _faker = new();

    [Fact]
    public void ToResponse_ShouldMapAllPropertiesCorrectly()
    {
        var category = new Category(Guid.NewGuid(), Guid.NewGuid(), _faker.Lorem.Word(), "#000000");

        var result = category.ToResponse();

        result.Id.Should().Be(category.Id);
        result.UserId.Should().Be(category.UserId);
        result.Name.Should().Be(category.Name);
        result.Color.Should().Be(category.Color);
        result.Keywords.Should().BeEquivalentTo(category.Keywords);
        result.CreatedAt.Should().Be(category.CreatedAt);
        result.UpdatedAt.Should().Be(category.UpdatedAt);
    }

    [Fact]
    public void ApplyUpdate_WhenNameChanged_ShouldReturnTrueAndUpdate()
    {
        var id = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var color = "#000000";
        var keywords = new List<string>();
        var category = new Category(id, userId, _faker.Lorem.Word(), color)
        {
            Keywords = keywords,
        };
        var currentCreatedTime = category.CreatedAt;
        var currentUpdatedTime = category.UpdatedAt;
        var newName = $"{category.Name} changed";
        var request = new CategoryRequest(newName, color, keywords);

        category.ApplyUpdate(request).Should().BeTrue();
        AssertCategoryFields(category, id, userId, newName, color, keywords, currentCreatedTime, currentUpdatedTime);
    }

    [Fact]
    public void ApplyUpdate_WhenColorChanged_ShouldReturnTrueAndUpdate()
    {
        var id = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var name = _faker.Lorem.Word();
        var keywords = new List<string>();
        var category = new Category(id, userId, name, "#000000")
        {
            Keywords = keywords,
        };
        var currentCreatedTime = category.CreatedAt;
        var currentUpdatedTime = category.UpdatedAt;
        var newColor = "#ffffff";
        var request = new CategoryRequest(name, newColor, keywords);

        category.ApplyUpdate(request).Should().BeTrue();
        AssertCategoryFields(category, id, userId, name, newColor, keywords, currentCreatedTime, currentUpdatedTime);
    }

    [Fact]
    public void ApplyUpdate_WhenKeywordsChanged_ShouldReturnTrueAndUpdate()
    {
        var id = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var name = _faker.Lorem.Word();
        var color = "#000000";
        var category = new Category(id, userId, name, color);

        var currentCreatedTime = category.CreatedAt;
        var currentUpdatedTime = category.UpdatedAt;
        var newKeywords = _faker.Lorem.Words().ToList();
        var request = new CategoryRequest(name, color, newKeywords);

        category.ApplyUpdate(request).Should().BeTrue();
        AssertCategoryFields(category, id, userId, name, color, newKeywords, currentCreatedTime, currentUpdatedTime);
    }

    [Fact]
    public void ApplyUpdate_WhenKeywordsNull_ShouldNotUpdateKeywords()
    {
        var id = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var name = _faker.Lorem.Word();
        var color = "#000000";
        var keywords = _faker.Lorem.Words().ToList();
        var category = new Category(id, userId, name, color)
        {
            Keywords = keywords  
        };

        var currentCreatedTime = category.CreatedAt;
        var currentUpdatedTime = category.UpdatedAt;
        var request = new CategoryRequest(name, color, null);

        var result = category.ApplyUpdate(request);

        result.Should().BeFalse();
        AssertCategoryFields(category, id, userId, name, color, keywords, currentCreatedTime, currentUpdatedTime);
    }

    [Fact]
    public void ApplyUpdate_NoChanges_ShouldReturnFalse()
    {
        var id = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var name = _faker.Lorem.Word();
        var color = "#000000";
        var keywords = _faker.Lorem.Words().ToList();
        var category = new Category(id, userId, name, color)
        {
            Keywords = keywords,
        };

        var currentCreatedTime = category.CreatedAt;
        var currentUpdatedTime = category.UpdatedAt;
        var request = new CategoryRequest(name, color, keywords);

        category.ApplyUpdate(request).Should().BeFalse();
        AssertCategoryFields(category, id, userId, name, color, keywords, currentCreatedTime, currentUpdatedTime);
    }
    
    private static void AssertCategoryFields(
        Category category,
        Guid id,
        Guid userId,
        string name,
        string color,
        List<string> keywords,
        DateTime createdAt,
        DateTime updatedAt)
    {
        category.Id.Should().Be(id);
        category.UserId.Should().Be(userId);
        category.Name.Should().Be(name);
        category.Color.Should().Be(color);
        category.Keywords.Should().BeEquivalentTo(keywords);
        category.CreatedAt.Should().Be(createdAt);
        category.UpdatedAt.Should().Be(updatedAt);
    }
}
