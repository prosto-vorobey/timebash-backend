using FluentAssertions;
using Moq;
using Timebash.Application.Extensions;
using Timebash.Core.DTOs.Responses;
using Timebash.Core.Entities;
using Timebash.Core.Exceptions;

namespace Timebash.Tests.Unit.Application.Services.MeService;

public class GetCategoriesAsyncTests : MeServiceTestsBase
{
    [Fact]
    public async Task GetCategories_ShouldReturnResponse()
    {
        var userId = Guid.NewGuid();
        var categories = new List<Category>
        {
            new(Guid.NewGuid(), userId, Faker.Lorem.Word(), "#000000")
        };
        var expected = new CategoriesListResponse([.. categories.Select(category => category.ToResponse())]);

        UserRepositoryMock.Setup(repository => repository.ExistsAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        CategoryRepositoryMock.Setup(repository => repository.GetByUserIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(categories);

        var result = await Service.GetCategoriesAsync(userId, CancellationToken.None);
        result.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public async Task GetCategories_EmptyId_ShouldThrowBadRequest()
        => await FluentActions
            .Awaiting(() => Service.GetCategoriesAsync(Guid.Empty, CancellationToken.None))
            .Should()
            .ThrowAsync<BadRequestException>()
            .WithMessage("Invalid id");

    [Fact]
    public async Task GetCategories_UserNotFound_ShouldThrowNotFound()
    {
        var id = Guid.NewGuid();
        UserRepositoryMock.Setup(repository => repository.ExistsAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(false);

        await FluentActions
            .Awaiting(() => Service.GetCategoriesAsync(id, CancellationToken.None))
            .Should()
            .ThrowAsync<NotFoundException>();
    }
}
