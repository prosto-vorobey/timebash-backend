using FluentAssertions;
using Moq;
using Timebash.Core.DTOs.Requests;
using Timebash.Core.Entities;
using Timebash.Core.Exceptions;

namespace Timebash.Tests.Unit.Application.Services.MeService;

public class UpdateEmailAsyncTests : MeServiceTestsBase
{
    [Fact]
    public async Task UpdateEmail_ValidRequest_ShouldReturnTrueAndUpdate()
    {
        var id = Guid.NewGuid();
        var name = Faker.Internet.UserName();
        var user = new User(id, name, Faker.Internet.Email());
        var currentCreatedTime = user.CreatedAt;
        var request = new UserEmailUpdateRequest($"{user.Email} changed");

        SetupUserExistsByEmail(request.Email);
        SetupUserEnsureAccess(user);

        var result = await Service.UpdateEmailAsync(request, id, CancellationToken.None);

        result.Should().BeTrue();
        user.Id.Should().Be(id);
        user.Name.Should().Be(name);
        user.Email.Should().Be(request.Email);
        user.PasswordHash.Should().BeNull();
        user.CreatedAt.Should().Be(currentCreatedTime);

        VerifyUserExistsByEmailCalled(request.Email);
        VerifyEnsureAccessCalled(id);
        VerifySaveChangesCalled();;
    }

    [Fact]
    public async Task UpdateEmail_NoChanges_ShouldReturnFalse()
    {
        var id = Guid.NewGuid();
        var name = Faker.Internet.UserName();
        var email = Faker.Internet.Email();
        var user = new User(id, name, email);
        var currentCreatedTime = user.CreatedAt;
        var request = new UserEmailUpdateRequest(user.Email);

        SetupUserExistsByEmail(request.Email);
        SetupUserEnsureAccess(user);

        var result = await Service.UpdateEmailAsync(request, id, CancellationToken.None);

        result.Should().BeFalse();
        user.Id.Should().Be(id);
        user.Name.Should().Be(name);
        user.Email.Should().Be(email);
        user.PasswordHash.Should().BeNull();
        user.CreatedAt.Should().Be(currentCreatedTime);

        VerifyUserExistsByEmailCalled(request.Email);
        VerifyEnsureAccessCalled(id);
        VerifySaveChangesNotCalled();
    }

    [Fact]
    public async Task UpdateEmail_EmailAlreadyExists_ShouldThrowResourceConflictException()
    {
        var user = new User(Guid.NewGuid(), Faker.Internet.UserName(), Faker.Internet.Email());
        var request = new UserEmailUpdateRequest(Faker.Internet.Email());

        SetupUserExistsByEmail(request.Email, true);
        SetupUserEnsureAccess(user);

        var exception = await FluentActions
            .Awaiting(() => Service.UpdateEmailAsync(request, user.Id, CancellationToken.None))
            .Should()
            .ThrowAsync<ResourceConflictException>();
        exception.Which.Field.Should().Be("Email");

        VerifyUserExistsByEmailCalled(request.Email);
        VerifyEnsureAccessNotCalled();
        VerifySaveChangesNotCalled();
    }

    [Fact]
    public async Task UpdateEmail_EmptyId_ShouldThrowBadRequest()
    {
        var id = Guid.Empty;
        var request = new UserEmailUpdateRequest(Faker.Internet.Email());

        SetupUserExistsByEmail(request.Email);
        SetupUserEnsureAccessThrowsBadRequest(id);

        await FluentActions
            .Awaiting(() => Service.UpdateEmailAsync(request, id, CancellationToken.None))
            .Should()
            .ThrowAsync<BadRequestException>();

        VerifyUserExistsByEmailCalled(request.Email);
        VerifyEnsureAccessCalled(id);
        VerifySaveChangesNotCalled();
    }

    [Fact]
    public async Task UpdateEmail_UserNotFound_ShouldThrowNotFound()
    {
        var id = Guid.NewGuid();
        var request = new UserEmailUpdateRequest(Faker.Internet.Email());

        SetupUserExistsByEmail(request.Email);
        SetupUserEnsureAccessThrowsNotFound(id);

        await FluentActions
            .Awaiting(() => Service.UpdateEmailAsync(request, id, CancellationToken.None))
            .Should()
            .ThrowAsync<NotFoundException>();

        VerifyUserExistsByEmailCalled(request.Email);
        VerifyEnsureAccessCalled(id);
        VerifySaveChangesNotCalled();
    }

    private void SetupUserExistsByEmail(string email, bool exists = false)
        => UserRepositoryMock.Setup(repository => repository.ExistsByEmailAsync(email, It.IsAny<CancellationToken>())).ReturnsAsync(exists);

    private void VerifyUserExistsByEmailCalled(string email)
        => UserRepositoryMock.Verify(repository => repository.ExistsByEmailAsync(email, It.IsAny<CancellationToken>()), Times.Once);
}
