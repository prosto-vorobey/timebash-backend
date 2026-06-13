using Bogus;
using FluentAssertions;
using Timebash.Application.Extensions.Requests;
using Timebash.Core.DTOs.Requests;

namespace Timebash.Tests.Unit.Application.Extensions.Requests;

public class RegisterRequestExtensionsTests
{
    private static readonly Faker _faker = new();

    [Fact]
    public void ToUser_ShouldMapAllPropertiesCorrectly()
    {
        var id = Guid.NewGuid();
        var request = new RegisterRequest(_faker.Internet.UserName(), _faker.Internet.Email(), _faker.Internet.Password());

        var result = request.ToUser(id);

        result.Id.Should().Be(id);
        result.Name.Should().Be(request.Name);
        result.Email.Should().Be(request.Email);
        result.PasswordHash.Should().BeNull();
    }

    [Fact]
    public void ToUser_ShouldNotCopyPassword()
    {
        var request = new RegisterRequest(_faker.Internet.UserName(), _faker.Internet.Email(), _faker.Internet.Password());

        var result = request.ToUser(Guid.NewGuid());
        result.PasswordHash.Should().NotBe(request.Password);
    }
}
