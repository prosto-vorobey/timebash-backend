using Moq;
using Timebash.Core.Entities;
using Timebash.Core.Exceptions;
using Timebash.Core.Services.Access;

namespace Timebash.Tests.Unit.Application.Services;

public static class CategoryAccessServiceMockExtensions
{
    public static void SetupEnsureAccess(this Mock<ICategoryAccessService> mock, Category category)
        => mock
            .Setup(service => service.EnsureAccessAsync(category.Id, category.UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

    public static void SetupEnsureAccessThrowsBadRequest(this Mock<ICategoryAccessService> mock, Guid id, Guid userId)
        => mock
            .Setup(service => service.EnsureAccessAsync(id, userId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new BadRequestException());

    public static void SetupEnsureAccessThrowsNotFound(this Mock<ICategoryAccessService> mock, Guid id, Guid userId)
        => mock
            .Setup(service => service.EnsureAccessAsync(id, userId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException());

    public static void SetupValidateAccess(this Mock<ICategoryAccessService> mock, Guid id, Guid userId)
        => mock
            .Setup(service => service.ValidateAccessAsync(id, userId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

    public static void SetupValidateAccessThrowsBadRequest(this Mock<ICategoryAccessService> mock, Guid id, Guid userId)
        => mock
            .Setup(service => service.ValidateAccessAsync(id, userId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new BadRequestException());

    public static void SetupValidateAccessThrowsNotFound(this Mock<ICategoryAccessService> mock, Guid id, Guid userId)
        => mock
            .Setup(service => service.ValidateAccessAsync(id, userId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException());

    public static void VerifyEnsureAccessCalled(this Mock<ICategoryAccessService> mock, Guid id, Guid userId)
        => mock.Verify(service => service.EnsureAccessAsync(id, userId, It.IsAny<CancellationToken>()), Times.Once);

    public static void VerifyValidateAccessCalled(this Mock<ICategoryAccessService> mock, Guid id, Guid userId)
        => mock.Verify(service => service.ValidateAccessAsync(id, userId, It.IsAny<CancellationToken>()), Times.Once);
}
