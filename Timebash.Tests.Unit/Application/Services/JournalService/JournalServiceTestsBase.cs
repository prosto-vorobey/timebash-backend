using Bogus;
using Moq;
using Timebash.Core.Contracts;
using Timebash.Core.Entities;
using Timebash.Core.Exceptions;
using Timebash.Core.Repositories;
using Timebash.Core.Services;
using Timebash.Core.Services.Access;
using Service = Timebash.Application.Services.JournalService;

namespace Timebash.Tests.Unit.Application.Services.JournalService;

public abstract class JournalServiceTestsBase
{
    public JournalServiceTestsBase()
    {
        UnitOfWorkMock = new();
        JournalRepositoryMock = new();
        ActivityRepositoryMock = new();
        SettingsRepositoryMock = new();
        AccessServiceMock = new();
        ActivityQueryServiceMock = new();

        Service = new(
            UnitOfWorkMock.Object,
            JournalRepositoryMock.Object,
            ActivityRepositoryMock.Object,
            SettingsRepositoryMock.Object,
            AccessServiceMock.Object,
            ActivityQueryServiceMock.Object
        );
    }

    protected static Faker Faker { get; } = new();
    protected Service Service { get; }
    protected Mock<IUnitOfWork> UnitOfWorkMock { get; }
    protected Mock<IJournalRepository> JournalRepositoryMock { get; }
    protected Mock<IActivityRepository> ActivityRepositoryMock { get; }
    protected Mock<IUserSettingsRepository> SettingsRepositoryMock { get; }
    protected Mock<IJournalAccessService> AccessServiceMock { get; }
    protected Mock<IActivityQueryService> ActivityQueryServiceMock { get; }

    protected void SetupEnsureAccess(Journal journal)
        => AccessServiceMock
            .Setup(service => service.EnsureAccessAsync(journal.Id, journal.UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(journal);

    protected void SetupEnsureAccessThrowsBadRequest(Guid id, Guid userId)
        => AccessServiceMock
            .Setup(service => service.EnsureAccessAsync(id, userId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new BadRequestException());

    protected void SetupEnsureAccessThrowsNotFound(Guid id, Guid userId)
        => AccessServiceMock
            .Setup(service => service.EnsureAccessAsync(id, userId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException());

    protected void SetupValidateAccess(Guid id, Guid userId)
        => AccessServiceMock
            .Setup(service => service.ValidateAccessAsync(id, userId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

    protected void SetupValidateAccessThrowsBadRequest(Guid id, Guid userId)
        => AccessServiceMock
            .Setup(service => service.ValidateAccessAsync(id, userId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new BadRequestException());

    protected void SetupValidateAccessThrowsNotFound(Guid id, Guid userId)
        => AccessServiceMock
            .Setup(service => service.ValidateAccessAsync(id, userId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException());

    protected void VerifyEnsureAccessCalled(Guid id, Guid userId)
        => AccessServiceMock.Verify(service => service.EnsureAccessAsync(id, userId, It.IsAny<CancellationToken>()), Times.Once);

    protected void VerifyValidateAccessCalled(Guid id, Guid userId)
        => AccessServiceMock.Verify(service => service.ValidateAccessAsync(id, userId, It.IsAny<CancellationToken>()), Times.Once);

    protected void VerifySaveChangesCalled()
        => UnitOfWorkMock.Verify(unit => unit.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);

    protected void VerifySaveChangesNotCalled()
        => UnitOfWorkMock.Verify(unit => unit.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
}
