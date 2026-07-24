using Bogus;
using Moq;
using Timebash.Core.Contracts;
using Timebash.Core.Repositories;
using Timebash.Core.Services.Access;
using Service = Timebash.Application.Services.CategoryService;

namespace Timebash.Tests.Unit.Application.Services.CategoryService;

public abstract class CategoryServiceTestsBase
{
    public CategoryServiceTestsBase()
    {
        UnitOfWorkMock = new(MockBehavior.Strict);
        RepositoryMock = new(MockBehavior.Strict);
        AccessServiceMock = new(MockBehavior.Strict);
        
        Service = new(UnitOfWorkMock.Object, RepositoryMock.Object, AccessServiceMock.Object);
    }

    protected static Faker Faker { get; } = new();
    protected Service Service { get; }
    protected Mock<IUnitOfWork> UnitOfWorkMock { get; }
    protected Mock<ICategoryRepository> RepositoryMock { get; }
    protected Mock<ICategoryAccessService> AccessServiceMock { get; }
}
