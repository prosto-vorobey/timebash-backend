using FluentAssertions;
using Moq;
using Timebash.Core.DTOs.Requests;
using Timebash.Core.Entities;
using Timebash.Core.Exceptions;
using Timebash.Tests.Unit.TestInfrastructure.MockExtensions;
using Timebash.Tests.Unit.TestInfrastructure.MockExtensions.AccessServices;

namespace Timebash.Tests.Unit.Application.Services.MeService;

public class UpdateNameAsyncTests : MeServiceTestsBase
{
    [Fact]
    public async Task UpdateName_ValidRequest_ShouldReturnTrueAndUpdate()
    {
        var id = Guid.NewGuid();
        var email = Faker.Internet.Email();
        var user = new User(id, Faker.Internet.UserName(), email);
        var currentCreatedTime = user.CreatedAt;
        var request = new UserNameUpdateRequest($"{user.Name} changed");

        SetupUserExistsByName(request.Name);
        UserAccessServiceMock.SetupEnsureAccess(user);
        UnitOfWorkMock.SetupSaveChanges();

        var result = await Service.UpdateNameAsync(request, id, CancellationToken.None);

        result.Should().BeTrue();
        user.Name.Should().Be(request.Name);
        user.Id.Should().Be(id);
        user.Email.Should().Be(email);
        user.PasswordHash.Should().BeNull();
        user.CreatedAt.Should().Be(currentCreatedTime);

        VerifyUserExistsByNameCalled(request.Name);
        UserAccessServiceMock.VerifyEnsureAccessCalled(id);
        UnitOfWorkMock.VerifySaveChangesCalled();
    }

    [Fact]
    public async Task UpdateName_NoChanges_ShouldReturnFalse()
    {
        var id = Guid.NewGuid();
        var name = Faker.Internet.UserName();
        var email = Faker.Internet.Email();
        var user = new User(id, name, email);
        var currentCreatedTime = user.CreatedAt;
        var request = new UserNameUpdateRequest(name);

        SetupUserExistsByName(request.Name);
        UserAccessServiceMock.SetupEnsureAccess(user);

        var result = await Service.UpdateNameAsync(request, id, CancellationToken.None);

        result.Should().BeFalse();
        user.Id.Should().Be(id);
        user.Name.Should().Be(name);
        user.Email.Should().Be(email);
        user.PasswordHash.Should().BeNull();
        user.CreatedAt.Should().Be(currentCreatedTime);

        VerifyUserExistsByNameCalled(request.Name);
        UserAccessServiceMock.VerifyEnsureAccessCalled(id);
    }

    [Fact]
    public async Task UpdateName_NameAlreadyExists_ShouldThrowResourceConflictException()
    {
        var user = new User(Guid.NewGuid(), Faker.Internet.UserName(), Faker.Internet.Email());
        var request = new UserNameUpdateRequest(Faker.Internet.UserName());

        SetupUserExistsByName(request.Name, true);
        UserAccessServiceMock.SetupEnsureAccess(user);

        var exception = await FluentActions
            .Awaiting(() => Service.UpdateNameAsync(request, Guid.NewGuid(), CancellationToken.None))
            .Should()
            .ThrowAsync<ResourceConflictException>();
        exception.Which.Field.Should().Be("Name");

        VerifyUserExistsByNameCalled(request.Name);
    }

    [Fact]
    public async Task UpdateName_EmptyId_ShouldThrowBadRequest()
    {
        var id = Guid.Empty;
        var request = new UserNameUpdateRequest(Faker.Internet.UserName());

        SetupUserExistsByName(request.Name);
        UserAccessServiceMock.SetupEnsureAccessThrowsBadRequest(id);

        await FluentActions
            .Awaiting(() => Service.UpdateNameAsync(request, id, CancellationToken.None))
            .Should()
            .ThrowAsync<BadRequestException>();

        VerifyUserExistsByNameCalled(request.Name);
        UserAccessServiceMock.VerifyEnsureAccessCalled(id);
    }

    [Fact]
    public async Task UpdateName_UserNotFound_ShouldThrowNotFound()
    {
        var id = Guid.NewGuid();
        var request = new UserNameUpdateRequest(Faker.Internet.UserName());

        SetupUserExistsByName(request.Name);
        UserAccessServiceMock.SetupEnsureAccessThrowsNotFound(id);

        await FluentActions
            .Awaiting(() => Service.UpdateNameAsync(request, id, CancellationToken.None))
            .Should()
            .ThrowAsync<NotFoundException>();

        VerifyUserExistsByNameCalled(request.Name);
        UserAccessServiceMock.VerifyEnsureAccessCalled(id);
    }

    private void SetupUserExistsByName(string name, bool exists = false)
        => UserRepositoryMock.Setup(repository => repository.ExistsByNameAsync(name, It.IsAny<CancellationToken>())).ReturnsAsync(exists);

    private void VerifyUserExistsByNameCalled(string name)
        => UserRepositoryMock.Verify(repository => repository.ExistsByNameAsync(name, It.IsAny<CancellationToken>()), Times.Once);
}
