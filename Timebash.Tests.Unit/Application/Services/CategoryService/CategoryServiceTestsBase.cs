using Bogus;
using Moq;
using Timebash.Core.Contracts;
using Timebash.Core.Repositories;
using Service = Timebash.Application.Services.CategoryService;

namespace Timebash.Tests.Unit.Application.Services.CategoryService;

public abstract class CategoryServiceTestsBase
{
    public CategoryServiceTestsBase()
    {
        UnitOfWorkMock = new();
        RepositoryMock = new();
        Service = new(UnitOfWorkMock.Object, RepositoryMock.Object);
    }

    protected static Faker Faker { get; } = new();
    protected Service Service { get; }
    protected Mock<IUnitOfWork> UnitOfWorkMock { get; }
    protected Mock<ICategoryRepository> RepositoryMock { get; }
}
