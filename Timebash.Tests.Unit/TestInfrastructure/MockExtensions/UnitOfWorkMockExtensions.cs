using Moq;
using Timebash.Core.Contracts;

namespace Timebash.Tests.Unit.TestInfrastructure.MockExtensions;

public static class UnitOfWorkMockExtensions
{
    public static void SetupSaveChanges(this Mock<IUnitOfWork> mock, int changesNumber = 1)
        => mock
            .Setup(unit => unit.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(changesNumber);

    public static void VerifySaveChangesCalled(this Mock<IUnitOfWork> mock)
        => mock.Verify(unit => unit.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
}