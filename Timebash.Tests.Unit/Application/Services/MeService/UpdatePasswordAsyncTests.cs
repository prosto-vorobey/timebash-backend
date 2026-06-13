using FluentAssertions;
using Moq;
using Timebash.Core.DTOs.Requests;
using Timebash.Core.Entities;
using Timebash.Core.Exceptions;

namespace Timebash.Tests.Unit.Application.Services.MeService;

public class UpdatePasswordAsyncTests : MeServiceTestsBase
{
    [Fact]
    public async Task UpdatePassword_ValidRequest_ShouldReturnTrueAndUpdate()
    {
        var id = Guid.NewGuid();
        var name = Faker.Internet.UserName();
        var email = Faker.Internet.Email();
        var currentPassword = Faker.Internet.Password();
        var user = new User(id, name, email);
        var currentCreatedTime = user.CreatedAt;
        var newPasswordHash = Faker.Random.Hash();
        var request = new PasswordUpdateRequest(currentPassword, Faker.Internet.Password());

        UserRepositoryMock.Setup(repository => repository.GetByIdAsync(id)).ReturnsAsync(user);
        PasswordServiceMock.Setup(service => service.VerifyPassword(user, request.CurrentPassword)).Returns(true);
        PasswordServiceMock.Setup(service => service.HashPassword(user, request.NewPassword)).Returns(newPasswordHash);

        var result = await Service.UpdatePasswordAsync(request, id);

        result.Should().BeTrue();
        user.PasswordHash.Should().Be(newPasswordHash);
        user.Id.Should().Be(id);
        user.Name.Should().Be(name);
        user.Email.Should().Be(email);
        user.CreatedAt.Should().Be(currentCreatedTime);

        PasswordServiceMock.Verify(service => service.HashPassword(user, request.NewPassword), Times.Once);
        UnitOfWorkMock.Verify(unit => unit.SaveChangesAsync(), Times.Once);        
    }

    [Fact]
    public async Task UpdatePassword_NoChanges_ShouldReturnFalse()
    {
        var id = Guid.NewGuid();
        var name = Faker.Internet.UserName();
        var email = Faker.Internet.Email();
        var currentPassword = Faker.Internet.Password();
        var currentPasswordHash = Faker.Random.Hash();
        var user = new User(id, name, email)
        {
            PasswordHash = currentPasswordHash
        };

        var currentCreatedTime = user.CreatedAt;
        var request = new PasswordUpdateRequest(currentPassword, currentPassword);

        UserRepositoryMock.Setup(repository => repository.GetByIdAsync(id)).ReturnsAsync(user);
        PasswordServiceMock.Setup(service => service.VerifyPassword(user, request.CurrentPassword)).Returns(true);

        var result = await Service.UpdatePasswordAsync(request, id);

        result.Should().BeFalse();
        user.Id.Should().Be(id);
        user.Name.Should().Be(name);
        user.Email.Should().Be(email);
        user.PasswordHash.Should().Be(currentPasswordHash);
        user.CreatedAt.Should().Be(currentCreatedTime);

        PasswordServiceMock.Verify(service => service.HashPassword(It.IsAny<User>(), It.IsAny<string>()), Times.Never);
        UnitOfWorkMock.Verify(unit => unit.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task UpdatePassword_UserNotFound_ShouldThrowNotFound()
    {
        var id = Guid.NewGuid();
        var request = new PasswordUpdateRequest(Faker.Internet.Password(), Faker.Internet.Password());

        UserRepositoryMock.Setup(repository => repository.GetByIdAsync(id)).ReturnsAsync((User?)null);

        await FluentActions
            .Invoking(() => Service.UpdatePasswordAsync(request, id))
            .Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task UpdatePassword_InvalidCurrentPassword_ShouldThrowUnauthorized()
    {
        var user = new User(Guid.NewGuid(), Faker.Internet.UserName(), Faker.Internet.Email());
        var request = new PasswordUpdateRequest(Faker.Internet.Password(), Faker.Internet.Password());

        UserRepositoryMock.Setup(repository => repository.GetByIdAsync(user.Id)).ReturnsAsync(user);
        PasswordServiceMock.Setup(service => service.VerifyPassword(user, request.CurrentPassword)).Returns(false);

        await FluentActions
            .Invoking(() => Service.UpdatePasswordAsync(request, user.Id))
            .Should().ThrowAsync<UnauthorizedException>();
    }
}
