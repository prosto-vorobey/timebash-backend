using Bogus;
using Moq;
using Timebash.Core.Repositories;
using Timebash.Core.Services;
using Service = Timebash.Application.Services.StatisticService;

namespace Timebash.Tests.Unit.Application.Services.StatisticService;

public abstract class StatisticServiceTestsBase
{
    public StatisticServiceTestsBase()
    {
        UserRepositoryMock = new();
        JournalRepositoryMock = new();
        CategoryRepositoryMock = new();
        ActivityQueryServiceMock = new();

        Service = new(
            UserRepositoryMock.Object,
            JournalRepositoryMock.Object,
            CategoryRepositoryMock.Object,
            ActivityQueryServiceMock.Object);
    }

    protected static long DurationSecond { get; } = 86_400L;
    protected static Faker Faker { get; } = new();
    protected Service Service { get; }
    protected Mock<IUserRepository> UserRepositoryMock { get; }
    protected Mock<IJournalRepository> JournalRepositoryMock { get; }
    protected Mock<ICategoryRepository> CategoryRepositoryMock { get; }
    protected Mock<IActivityQueryService> ActivityQueryServiceMock { get; }
}
