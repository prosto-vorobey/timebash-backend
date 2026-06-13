using Bogus;
using FluentAssertions;
using Timebash.Core.Entities;
using Timebash.Core.Exceptions;
using Timebash.Tests.Unit.Core.TestData;

namespace Timebash.Tests.Unit.Core.Entities;

public class CategoryTests
{
    private static readonly Faker _faker = new();

    [Fact]
    public void Constructor_WithValidData_ShouldCreateCategory()
    {
        var id = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var name = _faker.Lorem.Word();
        var color = "#000000";

        var timeBeforeCreate = DateTime.UtcNow;
        var result = new Category(id, userId, name, color);

        result.Id.Should().Be(id);
        result.UserId.Should().Be(userId);
        result.Name.Should().Be(name);
        result.Color.Should().Be(color);
        result.CreatedAt.Should().BeOnOrAfter(timeBeforeCreate);
        result.CreatedAt.Should().Be(result.UpdatedAt);
    }

    [Fact]
    public void Constructor_EmptyId_ShouldThrowDomainValidationException()
        => FluentActions
            .Invoking(() => new Category(Guid.Empty, Guid.NewGuid(), _faker.Lorem.Word(), "#000000"))
            .Should()
            .Throw<DomainValidationException>();

    [Fact]
    public void Constructor_EmptyUserId_ShouldThrowDomainValidationException()
        => FluentActions
            .Invoking(() => new Category(Guid.NewGuid(), Guid.Empty, _faker.Lorem.Word(), "#000000"))
            .Should()
            .Throw<DomainValidationException>();

    [Fact]
    public void Constructor_EmptyName_ShouldThrowDomainValidationException()
        => FluentActions
            .Invoking(() => new Category(Guid.NewGuid(), Guid.NewGuid(), string.Empty, "#000000"))
            .Should()
            .Throw<DomainValidationException>();

    [Fact]
    public void Constructor_InvalidColor_ShouldThrowDomainValidationException()
        => FluentActions
            .Invoking(() => new Category(Guid.NewGuid(), Guid.NewGuid(), _faker.Lorem.Word(), "black"))
            .Should()
            .Throw<DomainValidationException>();

    [Fact]
    public void SetName_WithValidName_ShouldUpdateName()
    {
        var category = new Category(Guid.NewGuid(), Guid.NewGuid(), _faker.Lorem.Word(), "#000000");
        var newName = $"{category.Name} changed";
        category.Name = newName;
        category.Name.Should().Be(newName);
    }

    [Theory]
    [ClassData(typeof(NullOrWhitespaceStringData))]
    public void SetName_EmptyName_ShouldThrowDomainValidationException(string name)
    {
        var category = new Category(Guid.NewGuid(), Guid.NewGuid(), _faker.Lorem.Word(), "#000000");
        FluentActions
            .Invoking(() => category.Name = name)
            .Should()
            .Throw<DomainValidationException>();
    }

    [Fact]
    public void SetColor_WithValidColor_ShouldUpdateColor()
    {
        var category = new Category(Guid.NewGuid(), Guid.NewGuid(), _faker.Lorem.Word(), "#000000");
        var newColor = "#FFFFFF";

        category.Color = newColor;
        category.Color.Should().Be(newColor);
    }

    [Theory]
    [ClassData(typeof(NullOrWhitespaceStringData))]
    [ClassData(typeof(InvalidHexColorData))]
    public void SetColor_InvalidColor_ShouldThrowDomainValidationException(string color)
    {
        var category = new Category(Guid.NewGuid(), Guid.NewGuid(), _faker.Lorem.Word(), "#000000");
        FluentActions
            .Invoking(() => category.Color = color)
            .Should()
            .Throw<DomainValidationException>();
    }
}
