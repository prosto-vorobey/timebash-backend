using Bogus;
using Moq;
using Timebash.Core.Contracts;
using Timebash.Core.Repositories;
using Timebash.Core.Services;
using Timebash.Core.Services.Access;
using Service = Timebash.Application.Services.MeService;

namespace Timebash.Tests.Unit.Application.Services.MeService;

public abstract class MeServiceTestsBase
{
    public MeServiceTestsBase()
    {
        UnitOfWorkMock = new(MockBehavior.Strict);
        UserRepositoryMock = new(MockBehavior.Strict);
        SettingsRepositoryMock = new(MockBehavior.Strict);
        JournalRepositoryMock = new(MockBehavior.Strict);
        CategoryRepositoryMock = new(MockBehavior.Strict);
        UserAccessServiceMock = new(MockBehavior.Strict);
        JournalAccessServiceMock = new(MockBehavior.Strict);
        PasswordServiceMock = new(MockBehavior.Strict);

        Service = new(
            UnitOfWorkMock.Object,
            UserRepositoryMock.Object,
            SettingsRepositoryMock.Object,
            JournalRepositoryMock.Object,
            CategoryRepositoryMock.Object,
            UserAccessServiceMock.Object,
            JournalAccessServiceMock.Object,
            PasswordServiceMock.Object);
    }

    protected static Faker Faker { get; } = new();
    protected Service Service { get; }
    protected Mock<IUnitOfWork> UnitOfWorkMock { get; }
    protected Mock<IUserRepository> UserRepositoryMock { get; }
    protected Mock<IUserSettingsRepository> SettingsRepositoryMock { get; }
    protected Mock<IJournalRepository> JournalRepositoryMock { get; }
    protected Mock<ICategoryRepository> CategoryRepositoryMock { get; }
    protected Mock<IUserAccessService> UserAccessServiceMock { get; }
    protected Mock<IJournalAccessService> JournalAccessServiceMock { get; }
    protected Mock<IPasswordService> PasswordServiceMock { get; }
}
