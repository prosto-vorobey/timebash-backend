using Bogus;
using Moq;
using Timebash.Core.Contracts;
using Timebash.Core.Repositories;
using Timebash.Core.Services;
using Service = Timebash.Application.Services.MeService;

namespace Timebash.Tests.Unit.Application.Services.MeService;

public abstract class MeServiceTestsBase
{
    public MeServiceTestsBase()
    {
        UnitOfWorkMock = new();
        UserRepositoryMock = new();
        SettingsRepositoryMock = new();
        JournalRepositoryMock = new();
        CategoryRepositoryMock = new();
        PasswordServiceMock = new();

        Service = new(
            UnitOfWorkMock.Object,
            UserRepositoryMock.Object,
            SettingsRepositoryMock.Object,
            JournalRepositoryMock.Object,
            CategoryRepositoryMock.Object,
            PasswordServiceMock.Object);
    }

    protected static Faker Faker { get; } = new();
    protected Service Service { get; }
    protected Mock<IUnitOfWork> UnitOfWorkMock { get; }
    protected Mock<IUserRepository> UserRepositoryMock { get; }
    protected Mock<IUserSettingsRepository> SettingsRepositoryMock { get; }
    protected Mock<IJournalRepository> JournalRepositoryMock { get; }
    protected Mock<ICategoryRepository> CategoryRepositoryMock { get; }
    protected Mock<IPasswordService> PasswordServiceMock { get; }
}
