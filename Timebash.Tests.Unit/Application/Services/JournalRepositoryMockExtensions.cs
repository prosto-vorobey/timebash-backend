using Moq;
using Timebash.Core.Entities;
using Timebash.Core.Repositories;

namespace Timebash.Tests.Unit.Application.Services;

public static class JournalRepositoryMockExtensions
{
    public static void SetupGetById(this Mock<IJournalRepository> mock, Journal journal)
        => mock.Setup(repository => repository.GetByIdAsync(journal.Id, It.IsAny<CancellationToken>())).ReturnsAsync(journal);

    public static void SetupGetById(this Mock<IJournalRepository> mock, Guid id)
        => mock.Setup(repository => repository.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync((Journal?)null);

    public static void VerifyGetByIdCalled(this Mock<IJournalRepository> mock, Guid id)
        => mock.Verify(repository => repository.GetByIdAsync(id, It.IsAny<CancellationToken>()), Times.Once);
}
