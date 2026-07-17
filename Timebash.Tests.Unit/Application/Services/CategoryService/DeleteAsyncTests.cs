using FluentAssertions;
using Moq;
using Timebash.Core.Entities;
using Timebash.Core.Exceptions;

namespace Timebash.Tests.Unit.Application.Services.CategoryService;

public class DeleteAsyncTests : CategoryServiceTestsBase
{
    [Fact]
    public async Task Delete_ValidAccess_ShouldDeleteCategory()
    {
        var category = new Category(Guid.NewGuid(), Guid.NewGuid(), Faker.Lorem.Word(), "#000000");
        SetupEnsureAccess(category);

        await Service.DeleteAsync(category.Id, category.UserId, CancellationToken.None);

        VerifyEnsureAccessCalled(category.Id, category.UserId);
        RepositoryMock.Verify(repository => repository.Delete(category), Times.Once);
        VerifySaveChangesCalled();
    }

    [Fact]
    public async Task Delete_EmptyId_ShouldThrowBadRequest()
    {
        var id = Guid.Empty;
        var userId = Guid.NewGuid();

        SetupEnsureAccessThrowsBadRequest(id, userId);

        await FluentActions
            .Awaiting(() => Service.DeleteAsync(id, userId, CancellationToken.None))
            .Should()
            .ThrowAsync<BadRequestException>();

        VerifyEnsureAccessCalled(id, userId);
        VerifyDeleteNotCalled();
        VerifySaveChangesNotCalled();
    }

    [Fact]
    public async Task Delete_CategoryNotFound_ShouldThrowNotFound()
    {
        var id = Guid.NewGuid();
        var userId = Guid.NewGuid();

        SetupEnsureAccessThrowsNotFound(id, userId);

        await FluentActions
            .Awaiting(() => Service.DeleteAsync(id, userId, CancellationToken.None))
            .Should()
            .ThrowAsync<NotFoundException>();

        VerifyEnsureAccessCalled(id, userId);
        VerifyDeleteNotCalled();
        VerifySaveChangesNotCalled();
    }

    private void VerifyDeleteNotCalled()
        => RepositoryMock.Verify(repository => repository.Delete(It.IsAny<Category>()), Times.Never);
}
