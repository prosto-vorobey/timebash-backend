using Bogus;
using Moq;
using Timebash.Core.Services;
using Timebash.Core.Services.Access;
using Service = Timebash.Application.Services.StatisticService;

namespace Timebash.Tests.Unit.Application.Services.StatisticService;

public abstract class StatisticServiceTestsBase
{
    public StatisticServiceTestsBase()
    {
        UserAccessServiceMock = new();
        JournalAccessServiceMock = new();
        CategoryAccessServiceMock = new();
        ActivityQueryServiceMock = new();

        Service = new(
            UserAccessServiceMock.Object,
            JournalAccessServiceMock.Object,
            CategoryAccessServiceMock.Object,
            ActivityQueryServiceMock.Object);
    }

    protected static long DurationSecond { get; } = 86_400L;
    protected static Faker Faker { get; } = new();
    protected Service Service { get; }
    protected Mock<IUserAccessService> UserAccessServiceMock { get; }
    protected Mock<IJournalAccessService> JournalAccessServiceMock { get; }
    protected Mock<ICategoryAccessService> CategoryAccessServiceMock { get; }
    protected Mock<IActivityQueryService> ActivityQueryServiceMock { get; }
}
