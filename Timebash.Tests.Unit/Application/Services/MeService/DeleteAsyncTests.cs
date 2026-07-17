using FluentAssertions;
using Moq;
using Timebash.Core.Entities;
using Timebash.Core.Exceptions;

namespace Timebash.Tests.Unit.Application.Services.MeService;

public class DeleteAsyncTests : MeServiceTestsBase
{
    [Fact]
    public async Task Delete_ValidRequest_ShouldDeleteUser()
    {
        var user = new User(Guid.NewGuid(), Faker.Internet.UserName(), Faker.Internet.Email());

        SetupUserEnsureAccess(user);

        await Service.DeleteAsync(user.Id, CancellationToken.None);

        VerifyEnsureAccessCalled(user.Id);
        SettingsRepositoryMock.Verify(repository => repository.DeleteAsync(user.Id, It.IsAny<CancellationToken>()), Times.Once);
        UserRepositoryMock.Verify(repository => repository.Delete(user), Times.Once);
        VerifySaveChangesCalled();
    }

    [Fact]
    public async Task Delete_EmptyId_ShouldThrowBadRequest()
    {
        var id = Guid.Empty;

        SetupUserEnsureAccessThrowsBadRequest(id);

        await FluentActions
            .Awaiting(() => Service.DeleteAsync(id, CancellationToken.None))
            .Should()
            .ThrowAsync<BadRequestException>();

        VerifyEnsureAccessCalled(id);
        VerifySettingsDeleteNotCalled();
        VerifyUserDeleteNotCalled();
        VerifySaveChangesNotCalled();
    }

    [Fact]
    public async Task Delete_UserNotFound_ShouldThrowNotFound()
    {
        var id = Guid.NewGuid();

        SetupUserEnsureAccessThrowsNotFound(id);

        await FluentActions
            .Awaiting(() => Service.DeleteAsync(id, CancellationToken.None))
            .Should()
            .ThrowAsync<NotFoundException>();

        VerifyEnsureAccessCalled(id);
        VerifySettingsDeleteNotCalled();
        VerifyUserDeleteNotCalled();
        VerifySaveChangesNotCalled();
    }

    private void VerifyUserDeleteNotCalled()
        => UserRepositoryMock.Verify(repository => repository.Delete(It.IsAny<User>()), Times.Never);

    private void VerifySettingsDeleteNotCalled()
        => SettingsRepositoryMock.Verify(repository => repository.DeleteAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
}
