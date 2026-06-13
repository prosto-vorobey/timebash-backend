using Bogus;
using Moq;
using Timebash.Core.Contracts;
using Timebash.Core.Repositories;
using Service = Timebash.Application.Services.ActivityService;

namespace Timebash.Tests.Unit.Application.Services.ActivityService;

public abstract partial class ActivityServiceTestsBase
{
    public ActivityServiceTestsBase()
    {
        UnitOfWorkMock = new();
        ActivityRepositoryMock = new();
        CategoryRepositoryMock = new();
        JournalRepositoryMock = new();

        Service = new(
            UnitOfWorkMock.Object,
            ActivityRepositoryMock.Object,
            CategoryRepositoryMock.Object,
            JournalRepositoryMock.Object);
    }

    protected static Faker Faker { get; } = new();
    protected Service Service { get; }
    protected Mock<IUnitOfWork> UnitOfWorkMock { get; }
    protected Mock<IActivityRepository> ActivityRepositoryMock { get; }
    protected Mock<ICategoryRepository> CategoryRepositoryMock { get; }
    protected Mock<IJournalRepository> JournalRepositoryMock { get; }
}
