using FluentAssertions;
using Moq;
using Timebash.Application.Extensions;
using Timebash.Core.DTOs.Responses;
using Timebash.Core.Entities;
using Timebash.Core.Exceptions;

namespace Timebash.Tests.Unit.Application.Services.ActivityService;

public class GetCategoriesByActivityIdAsyncTests : ActivityServiceTestsBase
{
    [Fact]
    public async Task GetCategoriesByActivityId_ValidAccess_ShouldReturnResponse()
    {
        var activity = new Activity(Guid.NewGuid(), Guid.NewGuid(), DateTime.MinValue, DateTime.MaxValue);
        var userId = Guid.NewGuid();
        var categories = new List<Category>
        {
            new(Guid.NewGuid(), userId, Faker.Lorem.Word(), "#000000")
        };
        var expected = new CategoriesListResponse([.. categories.Select(category => category.ToResponse())]);

        ActivityRepositoryMock.Setup(repository => repository.IsOwnedByUserAsync(activity.Id, userId)).ReturnsAsync(true);
        ActivityRepositoryMock.Setup(repository => repository.GetCategoriesByActivityIdAsync(activity.Id)).ReturnsAsync(categories);

        var result = await Service.GetCategoriesByActivityIdAsync(activity.Id, userId);

        result.Should().BeEquivalentTo(expected, options => options.WithoutStrictOrdering());
    }

    [Fact]
    public async Task GetCategoriesByActivityId_EmptyId_ShouldThrowBadRequest()
        => await FluentActions
            .Awaiting(() => Service.GetCategoriesByActivityIdAsync(Guid.Empty, Guid.NewGuid()))
            .Should()
            .ThrowAsync<BadRequestException>();

    [Fact]
    public async Task GetCategoriesByActivityId_ActivityNotFound_ShouldThrowNotFound()
    {
        var id = Guid.NewGuid();
        var userId = Guid.NewGuid();
        ActivityRepositoryMock.Setup(repository => repository.IsOwnedByUserAsync(id, userId)).ReturnsAsync(false);

        await FluentActions
            .Awaiting(() => Service.GetCategoriesByActivityIdAsync(id, userId))
            .Should()
            .ThrowAsync<NotFoundException>();
    }
}
