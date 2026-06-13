using Bogus;
using FluentAssertions;
using Timebash.Application.Extensions;
using Timebash.Core.Entities;

namespace Timebash.Tests.Unit.Application.Extensions;

public class UserExtensionsTests
{
    private static readonly Faker _faker = new();

    [Fact]
    public void ToResponse_ShouldMapAllPropertiesCorrectly()
    {
        var user = new User(Guid.NewGuid(), _faker.Internet.UserName(), _faker.Internet.Email());

        var result = user.ToResponse();

        result.Id.Should().Be(user.Id);
        result.Name.Should().Be(user.Name);
        result.Email.Should().Be(user.Email);
        result.CreatedAt.Should().Be(user.CreatedAt);
    }
}
