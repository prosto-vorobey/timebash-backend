using Bogus;
using Moq;
using Timebash.Core.Contracts;
using Timebash.Core.Repositories;
using Timebash.Core.Services;
using Timebash.Core.Services.Access;
using Service = Timebash.Application.Services.JournalService;

namespace Timebash.Tests.Unit.Application.Services.JournalService;

public abstract class JournalServiceTestsBase
{
    public JournalServiceTestsBase()
    {
        UnitOfWorkMock = new(MockBehavior.Strict);
        JournalRepositoryMock = new(MockBehavior.Strict);
        ActivityRepositoryMock = new(MockBehavior.Strict);
        SettingsRepositoryMock = new(MockBehavior.Strict);
        AccessServiceMock = new(MockBehavior.Strict);
        ActivityQueryServiceMock = new(MockBehavior.Strict);

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
}
