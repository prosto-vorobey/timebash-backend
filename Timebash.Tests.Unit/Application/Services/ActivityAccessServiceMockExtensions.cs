using Moq;
using Timebash.Core.Entities;
using Timebash.Core.Exceptions;
using Timebash.Core.Services.Access;

namespace Timebash.Tests.Unit.Application.Services;

public static class ActivityAccessServiceMockExtensions
{
    public static void SetupEnsureAccess(this Mock<IActivityAccessService> mock, Activity activity, Guid userId)
        => mock
            .Setup(service => service.EnsureAccessAsync(activity.Id, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(activity);

    public static void SetupEnsureAccessThrowsBadRequest(this Mock<IActivityAccessService> mock, Guid id, Guid userId)
        => mock
            .Setup(service => service.EnsureAccessAsync(id, userId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new BadRequestException());

    public static void SetupEnsureAccessThrowsNotFound(this Mock<IActivityAccessService> mock, Guid id, Guid userId)
        => mock
            .Setup(service => service.EnsureAccessAsync(id, userId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException());

    public static void SetupValidateAccess(this Mock<IActivityAccessService> mock, Guid id, Guid userId)
        => mock
            .Setup(service => service.ValidateAccessAsync(id, userId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

    public static void SetupValidateAccessThrowsBadRequest(this Mock<IActivityAccessService> mock, Guid id, Guid userId)
        => mock
            .Setup(service => service.ValidateAccessAsync(id, userId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new BadRequestException());

    public static void SetupValidateAccessThrowsNotFound(this Mock<IActivityAccessService> mock, Guid id, Guid userId)
        => mock
            .Setup(service => service.ValidateAccessAsync(id, userId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException());

    public static void VerifyEnsureAccessCalled(this Mock<IActivityAccessService> mock, Guid id, Guid userId)
        => mock.Verify(service => service.EnsureAccessAsync(id, userId, It.IsAny<CancellationToken>()), Times.Once);

    public static void VerifyValidateAccessCalled(this Mock<IActivityAccessService> mock, Guid id, Guid userId)
        => mock.Verify(service => service.ValidateAccessAsync(id, userId, It.IsAny<CancellationToken>()), Times.Once);
}
