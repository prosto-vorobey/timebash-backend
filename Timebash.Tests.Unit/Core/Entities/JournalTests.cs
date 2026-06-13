using Bogus;
using FluentAssertions;
using Timebash.Core.Entities;
using Timebash.Core.Exceptions;
using Timebash.Tests.Unit.Core.TestData;

namespace Timebash.Tests.Unit.Core.Entities;

public class JournalTests
{
    private static readonly Faker _faker = new();

    [Fact]
    public void Constructor_WithValidData_ShouldCreateJournal()
    {
        var id = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var name = _faker.Lorem.Word();

        var timeBeforeCreate = DateTime.UtcNow;
        var result = new Journal(id, userId, name);

        result.Id.Should().Be(id);
        result.UserId.Should().Be(userId);
        result.Name.Should().Be(name);
        result.CreatedAt.Should().BeOnOrAfter(timeBeforeCreate);
        result.CreatedAt.Should().Be(result.UpdatedAt);
    }

    [Fact]
    public void Constructor_EmptyId_ShouldThrowDomainValidationException()
        => FluentActions
            .Invoking(() => new Journal(Guid.Empty, Guid.NewGuid(), _faker.Lorem.Word()))
            .Should()
            .Throw<DomainValidationException>();

    [Fact]
    public void Constructor_EmptyUserId_ShouldThrowDomainValidationException()
        => FluentActions
            .Invoking(() => new Journal(Guid.NewGuid(), Guid.Empty, _faker.Lorem.Word()))
            .Should()
            .Throw<DomainValidationException>();

    [Fact]
    public void Constructor_EmptyName_ShouldThrowDomainValidationException()
        => FluentActions
            .Invoking(() => new Journal(Guid.NewGuid(), Guid.NewGuid(), string.Empty))
            .Should()
            .Throw<DomainValidationException>();
    
    [Fact]
    public void SetName_WithValidName_ShouldUpdateName()
    {
        var journal = new Journal(Guid.NewGuid(), Guid.NewGuid(), _faker.Lorem.Word());
        var newName = $"{journal.Name} changed";

        journal.Name = newName;
        journal.Name.Should().Be(newName);
    }

    [Theory]
    [ClassData(typeof(NullOrWhitespaceStringData))]
    public void SetName_EmptyName_ShouldThrowDomainValidationException(string name)
    {
        var journal = new Journal(Guid.NewGuid(), Guid.NewGuid(), _faker.Lorem.Word());
        FluentActions
            .Invoking(() => journal.Name = name)
            .Should()
            .Throw<DomainValidationException>();
    }
}
