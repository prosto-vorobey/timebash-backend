using Bogus;
using Moq;
using Timebash.Core.Contracts;
using Timebash.Core.Entities;
using Timebash.Core.Exceptions;
using Timebash.Core.Repositories;
using Timebash.Core.Services;
using Timebash.Core.Services.Access;
using Service = Timebash.Application.Services.MeService;

namespace Timebash.Tests.Unit.Application.Services.MeService;

public abstract class MeServiceTestsBase
{
    public MeServiceTestsBase()
    {
        UnitOfWorkMock = new();
        UserRepositoryMock = new();
        SettingsRepositoryMock = new();
        JournalRepositoryMock = new();
        CategoryRepositoryMock = new();
        UserAccessServiceMock = new();
        JournalAccessServiceMock = new();
        PasswordServiceMock = new();

        Service = new(
            UnitOfWorkMock.Object,
            UserRepositoryMock.Object,
            SettingsRepositoryMock.Object,
            JournalRepositoryMock.Object,
            CategoryRepositoryMock.Object,
            UserAccessServiceMock.Object,
            JournalAccessServiceMock.Object,
            PasswordServiceMock.Object);
    }

    protected static Faker Faker { get; } = new();
    protected Service Service { get; }
    protected Mock<IUnitOfWork> UnitOfWorkMock { get; }
    protected Mock<IUserRepository> UserRepositoryMock { get; }
    protected Mock<IUserSettingsRepository> SettingsRepositoryMock { get; }
    protected Mock<IJournalRepository> JournalRepositoryMock { get; }
    protected Mock<ICategoryRepository> CategoryRepositoryMock { get; }
    protected Mock<IUserAccessService> UserAccessServiceMock { get; }
    protected Mock<IJournalAccessService> JournalAccessServiceMock { get; }
    protected Mock<IPasswordService> PasswordServiceMock { get; }

    protected void SetupUserEnsureAccess(User user)
        => UserAccessServiceMock
            .Setup(service => service.EnsureAccessAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

    protected void SetupUserEnsureAccessThrowsBadRequest(Guid id)
        => UserAccessServiceMock
            .Setup(service => service.EnsureAccessAsync(id, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new BadRequestException());

    protected void SetupUserEnsureAccessThrowsNotFound(Guid id)
        => UserAccessServiceMock
            .Setup(service => service.EnsureAccessAsync(id, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException());

    protected void SetupUserValidateExists(Guid id)
        => UserAccessServiceMock
            .Setup(service => service.ValidateExistsAsync(id, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

    protected void SetupUserValidateExistsThrowsBadRequest(Guid id)
        => UserAccessServiceMock
            .Setup(service => service.ValidateExistsAsync(id, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new BadRequestException());

    protected void SetupUserValidateExistsThrowsNotFound(Guid id)
        => UserAccessServiceMock
            .Setup(service => service.ValidateExistsAsync(id, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException());

    protected void VerifyEnsureAccessCalled(Guid id)
    => UserAccessServiceMock.Verify(service => service.EnsureAccessAsync(id, It.IsAny<CancellationToken>()), Times.Once);

    protected void VerifyValidateExistsCalled(Guid id)
        => UserAccessServiceMock.Verify(service => service.ValidateExistsAsync(id, It.IsAny<CancellationToken>()), Times.Once);

    protected void VerifySaveChangesCalled()
        => UnitOfWorkMock.Verify(unit => unit.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);

    protected void VerifyEnsureAccessNotCalled()
        => UserAccessServiceMock.Verify(service => service.EnsureAccessAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);

    protected void VerifySaveChangesNotCalled()
        => UnitOfWorkMock.Verify(unit => unit.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
}
