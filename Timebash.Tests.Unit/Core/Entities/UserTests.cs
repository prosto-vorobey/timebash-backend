using Bogus;
using FluentAssertions;
using Timebash.Core.Entities;
using Timebash.Core.Exceptions;
using Timebash.Tests.Unit.Core.TestData;

namespace Timebash.Tests.Unit.Core.Entities;

public class UserTests
{
    private static readonly Faker _faker = new();

    [Fact]
    public void Constructor_WithValidData_ShouldCreateUser()
    {
        var id = Guid.NewGuid();
        var name = _faker.Internet.UserName();
        var email = _faker.Internet.Email();

        var timeBeforeCreate = DateTime.UtcNow;
        var result = new User(id, name, email);

        result.Id.Should().Be(id);
        result.Name.Should().Be(name);
        result.Email.Should().Be(email);
        result.PasswordHash.Should().BeNull();
        result.CreatedAt.Should().BeOnOrAfter(timeBeforeCreate);
    }

    [Fact]
    public void Constructor_EmptyId_ShouldThrowDomainValidationException()
        => FluentActions
            .Invoking(() => new User(Guid.Empty, _faker.Internet.UserName(), _faker.Internet.Email()))
            .Should()
            .Throw<DomainValidationException>();

    [Fact]
    public void Constructor_EmptyName_ShouldThrowDomainValidationException()
        => FluentActions
            .Invoking(() => new User(Guid.NewGuid(), string.Empty, _faker.Internet.Email()))
            .Should()
            .Throw<DomainValidationException>();

    [Fact]
    public void Constructor_EmptyEmail_ShouldThrowDomainValidationException()
        => FluentActions
            .Invoking(() => new User(Guid.NewGuid(), _faker.Internet.UserName(), string.Empty))
            .Should()
            .Throw<DomainValidationException>();

    [Fact]
    public void SetName_WithValidName_ShouldUpdateName()
    {
        var user = new User(Guid.NewGuid(), _faker.Internet.UserName(), _faker.Internet.Email());
        var newName = $"{user.Name} changed";

        user.Name = newName;
        user.Name.Should().Be(newName);
    }

    [Theory]
    [ClassData(typeof(NullOrWhitespaceStringData))]
    public void SetName_EmptyName_ShouldThrowDomainValidationException(string name)
    {
        var user = new User(Guid.NewGuid(), _faker.Internet.UserName(), _faker.Internet.Email());
        FluentActions
            .Invoking(() => user.Name = name)
            .Should()
            .Throw<DomainValidationException>();
    }

    [Fact]
    public void SetEmail_WithValidEmail_ShouldUpdateName()
    {
        var user = new User(Guid.NewGuid(), _faker.Internet.UserName(), _faker.Internet.Email());
        var newEmail = _faker.Internet.Email();

        user.Email = newEmail;
        user.Email.Should().Be(newEmail);
    }

    [Theory]
    [ClassData(typeof(NullOrWhitespaceStringData))]
    public void SetEmail_EmptyEmail_ShouldThrowDomainValidationException(string email)
    {
        var user = new User(Guid.NewGuid(), _faker.Internet.UserName(), _faker.Internet.Email());
        FluentActions
            .Invoking(() => user.Email = email)
            .Should()
            .Throw<DomainValidationException>();
    }

    [Fact]
    public void SetPasswordHash_WithValidHash_ShouldUpdatePasswordHash()
    {
        var user = new User(Guid.NewGuid(), _faker.Internet.UserName(), _faker.Internet.Email());
        var newHash = _faker.Random.Hash();

        user.PasswordHash = newHash;

        user.PasswordHash.Should().Be(newHash);
    }

    [Theory]
    [ClassData(typeof(NullOrWhitespaceStringData))]
    public void SetPasswordHash_EmptyHash_ShouldThrowDomainValidationException(string hash)
    {
        var user = new User(Guid.NewGuid(), _faker.Internet.UserName(), _faker.Internet.Email());

        FluentActions
            .Invoking(() => user.PasswordHash = hash)
            .Should()
            .Throw<DomainValidationException>();
    }
}
