using Moq;
using Timebash.Core.Entities;
using Timebash.Core.Repositories;

namespace Timebash.Tests.Unit.TestInfrastructure.MockExtensions.Repositories;

public static class UserSettingsRepositoryMockExtensions
{
    public static void SetupGetById(this Mock<IUserSettingsRepository> mock, UserSettings userSettings)
        => mock
            .Setup(repository => repository.GetByIdAsync(userSettings.UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(userSettings);

    public static void SetupGetById(this Mock<IUserSettingsRepository> mock, Guid userId)
        => mock
            .Setup(repository => repository.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserSettings?)null);

    public static void VerifyGetByIdCalled(this Mock<IUserSettingsRepository> mock, Guid userId)
        => mock.Verify(repository => repository.GetByIdAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
}
