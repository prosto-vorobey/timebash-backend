using FluentAssertions;
using Moq;
using Timebash.Application.Extensions;
using Timebash.Core.DTOs.Responses;
using Timebash.Core.Entities;
using Timebash.Core.Exceptions;

namespace Timebash.Tests.Unit.Application.Services.CategoryService;

public class GetActivitiesByCategoryIdAsyncTests : CategoryServiceTestsBase
{
    [Fact]
    public async Task GetActivitiesByCategoryId_ValidAccess_ShouldReturnResponse()
    {
        var category = new Category(Guid.NewGuid(), Guid.NewGuid(), Faker.Lorem.Word(), "#000000");
        var activities = new List<Activity>
        {
            new(Guid.NewGuid(), Guid.NewGuid(), DateTime.MinValue, DateTime.MaxValue)
        };
        var expected = new ActivitiesListResponse([.. activities.Select(activity => activity.ToResponse())]);

        RepositoryMock.Setup(repository => repository.IsUserLinkedAsync(category.Id, category.UserId)).ReturnsAsync(true);
        RepositoryMock.Setup(repository => repository.GetActivitiesByCategoryIdAsync(category.Id)).ReturnsAsync(activities);

        var result = await Service.GetActivitiesByCategoryIdAsync(category.Id, category.UserId);
        result.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public async Task GetActivitiesByCategoryId_EmptyId_ShouldThrowBadRequest()
        => await FluentActions
            .Awaiting(() => Service.GetActivitiesByCategoryIdAsync(Guid.Empty, Guid.NewGuid()))
            .Should()
            .ThrowAsync<BadRequestException>();

    [Fact]
    public async Task GetActivitiesByCategoryId_CategoryNotFound_ShouldThrowNotFound()
    {
        var id = Guid.NewGuid();
        var userId = Guid.NewGuid();
        RepositoryMock.Setup(repository => repository.IsUserLinkedAsync(id, userId)).ReturnsAsync(false);

        await FluentActions
            .Awaiting(() => Service.GetActivitiesByCategoryIdAsync(id, userId))
            .Should()
            .ThrowAsync<NotFoundException>();
    }
}
