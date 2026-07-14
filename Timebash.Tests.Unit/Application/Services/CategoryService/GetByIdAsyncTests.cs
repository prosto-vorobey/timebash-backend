using FluentAssertions;
using Moq;
using Timebash.Application.Extensions;
using Timebash.Core.Entities;
using Timebash.Core.Exceptions;

namespace Timebash.Tests.Unit.Application.Services.CategoryService;

public class GetByIdAsyncTests : CategoryServiceTestsBase
{
    [Fact]
    public async Task GetById_ValidAccess_ShouldReturnResponse()
    {
        var category = new Category(Guid.NewGuid(), Guid.NewGuid(), Faker.Lorem.Word(), "#000000")
        {
            Keywords = [.. Faker.Lorem.Words()]
        };
        var expected = category.ToResponse();

        RepositoryMock.Setup(repository => repository.GetByIdAsync(category.Id, It.IsAny<CancellationToken>())).ReturnsAsync(category);

        var result = await Service.GetByIdAsync(category.Id, category.UserId, CancellationToken.None);
        result.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public async Task GetById_EmptyId_ShouldThrowBadRequest()
        => await FluentActions
            .Awaiting(() => Service.GetByIdAsync(Guid.Empty, Guid.NewGuid(), CancellationToken.None))
            .Should()
            .ThrowAsync<BadRequestException>();

    [Fact]
    public async Task GetById_CategoryNotFound_ShouldThrowNotFound()
    {
        var id = Guid.NewGuid();
        RepositoryMock.Setup(repository => repository.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync((Category?)null);

        await FluentActions
            .Awaiting(() => Service.GetByIdAsync(id, Guid.NewGuid(), CancellationToken.None))
            .Should()
            .ThrowAsync<NotFoundException>();
    }
}
