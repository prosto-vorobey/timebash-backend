using Bogus;
using Moq;
using Timebash.Core.Contracts;
using Timebash.Core.Repositories;
using Timebash.Core.Services;
using Service = Timebash.Application.Services.JournalService;

namespace Timebash.Tests.Unit.Application.Services.JournalService;

public abstract class JournalServiceTestsBase
{
    public JournalServiceTestsBase()
    {
        UnitOfWorkMock = new();
        JournalRepositoryMock = new();
        ActivityRepositoryMock = new();
        SettingsRepositoryMock = new();
        ActivityQueryServiceMock = new();

        Service = new(
            UnitOfWorkMock.Object,
            JournalRepositoryMock.Object,
            ActivityRepositoryMock.Object,
            SettingsRepositoryMock.Object,
            ActivityQueryServiceMock.Object
        );
    }

    protected static Faker Faker { get; } = new();
    protected Service Service { get; }
    protected Mock<IUnitOfWork> UnitOfWorkMock { get; }
    protected Mock<IJournalRepository> JournalRepositoryMock { get; }
    protected Mock<IActivityRepository> ActivityRepositoryMock { get; }
    protected Mock<IUserSettingsRepository> SettingsRepositoryMock { get; }
    protected Mock<IActivityQueryService> ActivityQueryServiceMock { get; }
}
