using FluentAssertions;
using Moq;
using Timebash.Core.DTOs.Requests;
using Timebash.Core.Entities;
using Timebash.Core.Exceptions;
using Timebash.Tests.Unit.TestInfrastructure.MockExtensions;
using Timebash.Tests.Unit.TestInfrastructure.MockExtensions.AccessServices;

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

        UserAccessServiceMock.SetupEnsureAccess(user);
        SetupVerifyPassword(user, request.CurrentPassword);
        PasswordServiceMock.Setup(service => service.HashPassword(user, request.NewPassword)).Returns(newPasswordHash);
        UnitOfWorkMock.SetupSaveChanges();

        var result = await Service.UpdatePasswordAsync(request, id, CancellationToken.None);

        result.Should().BeTrue();
        user.PasswordHash.Should().Be(newPasswordHash);
        user.Id.Should().Be(id);
        user.Name.Should().Be(name);
        user.Email.Should().Be(email);
        user.CreatedAt.Should().Be(currentCreatedTime);

        UserAccessServiceMock.VerifyEnsureAccessCalled(id);
        VerifyVerifyPasswordCalled(user, request.CurrentPassword);
        VerifyHashPasswordCalled(user, request.NewPassword);
        UnitOfWorkMock.VerifySaveChangesCalled();
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

        UserAccessServiceMock.SetupEnsureAccess(user);
        SetupVerifyPassword(user, request.CurrentPassword);

        var result = await Service.UpdatePasswordAsync(request, id, CancellationToken.None);

        result.Should().BeFalse();
        user.Id.Should().Be(id);
        user.Name.Should().Be(name);
        user.Email.Should().Be(email);
        user.PasswordHash.Should().Be(currentPasswordHash);
        user.CreatedAt.Should().Be(currentCreatedTime);

        UserAccessServiceMock.VerifyEnsureAccessCalled(id);
        VerifyVerifyPasswordCalled(user, request.CurrentPassword);
    }

    [Fact]
    public async Task UpdatePassword_EmptyId_ShouldThrowBadRequest()
    {
        var id = Guid.Empty;
        UserAccessServiceMock.SetupEnsureAccessThrowsBadRequest(id);

        await FluentActions
            .Awaiting(() => Service.UpdatePasswordAsync(
                new(Faker.Internet.Password(), Faker.Internet.Password()),
                id,
                CancellationToken.None))
            .Should()
            .ThrowAsync<BadRequestException>();

        UserAccessServiceMock.VerifyEnsureAccessCalled(id);
    }

    [Fact]
    public async Task UpdatePassword_UserNotFound_ShouldThrowNotFound()
    {
        var id = Guid.NewGuid();
        var request = new PasswordUpdateRequest(Faker.Internet.Password(), Faker.Internet.Password());

        UserAccessServiceMock.SetupEnsureAccessThrowsNotFound(id);

        await FluentActions
            .Awaiting(() => Service.UpdatePasswordAsync(request, id, CancellationToken.None))
            .Should()
            .ThrowAsync<NotFoundException>();

        UserAccessServiceMock.VerifyEnsureAccessCalled(id);
    }

    [Fact]
    public async Task UpdatePassword_InvalidCurrentPassword_ShouldThrowUnauthorized()
    {
        var user = new User(Guid.NewGuid(), Faker.Internet.UserName(), Faker.Internet.Email());
        var request = new PasswordUpdateRequest(Faker.Internet.Password(), Faker.Internet.Password());

        UserAccessServiceMock.SetupEnsureAccess(user);
        SetupVerifyPassword(user, request.CurrentPassword, false);

        await FluentActions
            .Awaiting(() => Service.UpdatePasswordAsync(request, user.Id, CancellationToken.None))
            .Should()
            .ThrowAsync<UnauthorizedException>();

        UserAccessServiceMock.VerifyEnsureAccessCalled(user.Id);
        VerifyVerifyPasswordCalled(user, request.CurrentPassword);
    }

    private void SetupVerifyPassword(User user, string currentPassword, bool isValid = true)
        => PasswordServiceMock.Setup(service => service.VerifyPassword(user, currentPassword)).Returns(isValid);

    private void VerifyVerifyPasswordCalled(User user, string currentPassword)
        => PasswordServiceMock.Verify(service => service.VerifyPassword(user, currentPassword), Times.Once);

    private void VerifyHashPasswordCalled(User user, string newPassword)
        => PasswordServiceMock.Verify(service => service.HashPassword(user, newPassword), Times.Once);
}
