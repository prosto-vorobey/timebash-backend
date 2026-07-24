using FluentAssertions;
using Moq;
using Timebash.Core.Entities;
using Timebash.Core.Exceptions;
using Timebash.Tests.Unit.TestInfrastructure.MockExtensions;
using Timebash.Tests.Unit.TestInfrastructure.MockExtensions.AccessServices;

namespace Timebash.Tests.Unit.Application.Services.MeService;

public class DeleteAsyncTests : MeServiceTestsBase
{
    [Fact]
    public async Task Delete_ValidRequest_ShouldDeleteUser()
    {
        var user = new User(Guid.NewGuid(), Faker.Internet.UserName(), Faker.Internet.Email());

        UserAccessServiceMock.SetupEnsureAccess(user);
        SettingsRepositoryMock.Setup(repository => repository.DeleteAsync(user.Id, CancellationToken.None)).Returns(Task.CompletedTask);
        UserRepositoryMock.Setup(repository => repository.Delete(user));
        UnitOfWorkMock.SetupSaveChanges();

        await Service.DeleteAsync(user.Id, CancellationToken.None);

        UserAccessServiceMock.VerifyEnsureAccessCalled(user.Id);
        SettingsRepositoryMock.Verify(repository => repository.DeleteAsync(user.Id, It.IsAny<CancellationToken>()), Times.Once);
        UserRepositoryMock.Verify(repository => repository.Delete(user), Times.Once);
        UnitOfWorkMock.VerifySaveChangesCalled();
    }

    [Fact]
    public async Task Delete_EmptyId_ShouldThrowBadRequest()
    {
        var id = Guid.Empty;

        UserAccessServiceMock.SetupEnsureAccessThrowsBadRequest(id);

        await FluentActions
            .Awaiting(() => Service.DeleteAsync(id, CancellationToken.None))
            .Should()
            .ThrowAsync<BadRequestException>();

        UserAccessServiceMock.VerifyEnsureAccessCalled(id);
    }

    [Fact]
    public async Task Delete_UserNotFound_ShouldThrowNotFound()
    {
        var id = Guid.NewGuid();

        UserAccessServiceMock.SetupEnsureAccessThrowsNotFound(id);

        await FluentActions
            .Awaiting(() => Service.DeleteAsync(id, CancellationToken.None))
            .Should()
            .ThrowAsync<NotFoundException>();

        UserAccessServiceMock.VerifyEnsureAccessCalled(id);
    }
}
