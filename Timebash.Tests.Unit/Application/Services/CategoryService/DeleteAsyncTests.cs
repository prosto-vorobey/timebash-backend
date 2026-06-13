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
        RepositoryMock.Setup(repository => repository.GetByIdAsync(category.Id)).ReturnsAsync(category);

        await Service.DeleteAsync(category.Id, category.UserId);

        RepositoryMock.Verify(repository => repository.Delete(category), Times.Once);
        UnitOfWorkMock.Verify(unit => unit.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task Delete_EmptyId_ShouldThrowBadRequest()
        => await FluentActions
            .Awaiting(() => Service.DeleteAsync(Guid.Empty, Guid.NewGuid()))
            .Should()
            .ThrowAsync<BadRequestException>();

    [Fact]
    public async Task Delete_CategoryNotFound_ShouldThrowNotFound()
    {
        var id = Guid.NewGuid();
        RepositoryMock.Setup(repository => repository.GetByIdAsync(id)).ReturnsAsync((Category?)null);

        await FluentActions
            .Awaiting(() => Service.DeleteAsync(id, Guid.NewGuid()))
            .Should()
            .ThrowAsync<NotFoundException>();
    }
}
