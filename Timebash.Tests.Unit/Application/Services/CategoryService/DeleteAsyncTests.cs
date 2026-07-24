using FluentAssertions;
using Moq;
using Timebash.Core.Entities;
using Timebash.Core.Exceptions;
using Timebash.Tests.Unit.TestInfrastructure.MockExtensions;
using Timebash.Tests.Unit.TestInfrastructure.MockExtensions.AccessServices;

namespace Timebash.Tests.Unit.Application.Services.CategoryService;

public class DeleteAsyncTests : CategoryServiceTestsBase
{
    [Fact]
    public async Task Delete_ValidAccess_ShouldDeleteCategory()
    {
        var category = new Category(Guid.NewGuid(), Guid.NewGuid(), Faker.Lorem.Word(), "#000000");

        AccessServiceMock.SetupEnsureAccess(category);
        RepositoryMock.Setup(repository => repository.Delete(category));
        UnitOfWorkMock.SetupSaveChanges();

        await Service.DeleteAsync(category.Id, category.UserId, CancellationToken.None);

        AccessServiceMock.VerifyEnsureAccessCalled(category.Id, category.UserId);
        RepositoryMock.Verify(repository => repository.Delete(category), Times.Once);
        UnitOfWorkMock.VerifySaveChangesCalled();
    }

    [Fact]
    public async Task Delete_EmptyId_ShouldThrowBadRequest()
    {
        var id = Guid.Empty;
        var userId = Guid.NewGuid();

        AccessServiceMock.SetupEnsureAccessThrowsBadRequest(id, userId);

        await FluentActions
            .Awaiting(() => Service.DeleteAsync(id, userId, CancellationToken.None))
            .Should()
            .ThrowAsync<BadRequestException>();

        AccessServiceMock.VerifyEnsureAccessCalled(id, userId);
    }

    [Fact]
    public async Task Delete_CategoryNotFound_ShouldThrowNotFound()
    {
        var id = Guid.NewGuid();
        var userId = Guid.NewGuid();

        AccessServiceMock.SetupEnsureAccessThrowsNotFound(id, userId);

        await FluentActions
            .Awaiting(() => Service.DeleteAsync(id, userId, CancellationToken.None))
            .Should()
            .ThrowAsync<NotFoundException>();

        AccessServiceMock.VerifyEnsureAccessCalled(id, userId);
    }
}
