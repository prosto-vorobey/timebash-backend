using Moq;
using Timebash.Core.Entities;
using Timebash.Core.Exceptions;
using Timebash.Core.Services.Access;

namespace Timebash.Tests.Unit.TestInfrastructure.MockExtensions.AccessServices;

public static class UserAccessServiceMockExtensions
{
    public static void SetupEnsureAccess(this Mock<IUserAccessService> mock, User user)
        => mock
            .Setup(service => service.EnsureAccessAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

    public static void SetupEnsureAccessThrowsBadRequest(this Mock<IUserAccessService> mock, Guid id)
        => mock
            .Setup(service => service.EnsureAccessAsync(id, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new BadRequestException());

    public static void SetupEnsureAccessThrowsNotFound(this Mock<IUserAccessService> mock, Guid id)
        => mock
            .Setup(service => service.EnsureAccessAsync(id, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException());

    public static void SetupValidateExists(this Mock<IUserAccessService> mock, Guid id)
        => mock
            .Setup(service => service.ValidateExistsAsync(id, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

    public static void SetupValidateExistsThrowsBadRequest(this Mock<IUserAccessService> mock, Guid id)
        => mock
            .Setup(service => service.ValidateExistsAsync(id, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new BadRequestException());

    public static void SetupValidateExistsThrowsNotFound(this Mock<IUserAccessService> mock, Guid id)
        => mock
            .Setup(service => service.ValidateExistsAsync(id, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException());

    public static void VerifyEnsureAccessCalled(this Mock<IUserAccessService> mock, Guid id)
        => mock.Verify(service => service.EnsureAccessAsync(id, It.IsAny<CancellationToken>()), Times.Once);

    public static void VerifyValidateExistsCalled(this Mock<IUserAccessService> mock, Guid id)
        => mock.Verify(service => service.ValidateExistsAsync(id, It.IsAny<CancellationToken>()), Times.Once);
}
