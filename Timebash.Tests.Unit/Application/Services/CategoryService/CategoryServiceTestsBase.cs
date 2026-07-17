using Bogus;
using Moq;
using Timebash.Core.Contracts;
using Timebash.Core.Entities;
using Timebash.Core.Exceptions;
using Timebash.Core.Repositories;
using Timebash.Core.Services.Access;
using Service = Timebash.Application.Services.CategoryService;

namespace Timebash.Tests.Unit.Application.Services.CategoryService;

public abstract class CategoryServiceTestsBase
{
    public CategoryServiceTestsBase()
    {
        UnitOfWorkMock = new();
        RepositoryMock = new();
        AccessServiceMock = new();
        
        Service = new(UnitOfWorkMock.Object, RepositoryMock.Object, AccessServiceMock.Object);
    }

    protected static Faker Faker { get; } = new();
    protected Service Service { get; }
    protected Mock<IUnitOfWork> UnitOfWorkMock { get; }
    protected Mock<ICategoryRepository> RepositoryMock { get; }
    protected Mock<ICategoryAccessService> AccessServiceMock { get; }

    protected void SetupEnsureAccess(Category category)
        => AccessServiceMock
            .Setup(service => service.EnsureAccessAsync(category.Id, category.UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

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
