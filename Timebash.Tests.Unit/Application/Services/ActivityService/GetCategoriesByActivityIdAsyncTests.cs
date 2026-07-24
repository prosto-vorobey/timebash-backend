using FluentAssertions;
using Moq;
using Timebash.Application.Extensions;
using Timebash.Core.DTOs.Responses;
using Timebash.Core.Entities;
using Timebash.Core.Exceptions;
using Timebash.Tests.Unit.TestInfrastructure.MockExtensions.AccessServices;

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

        ActivityAccessServiceMock.SetupValidateAccess(activity.Id, userId);
        ActivityRepositoryMock
            .Setup(repository => repository.GetCategoriesByActivityIdAsync(activity.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(categories);

        var result = await Service.GetCategoriesByActivityIdAsync(activity.Id, userId, CancellationToken.None);

        result.Should().BeEquivalentTo(expected, options => options.WithoutStrictOrdering());
        ActivityAccessServiceMock.VerifyValidateAccessCalled(activity.Id, userId);
        ActivityRepositoryMock.Verify(repository => repository.GetCategoriesByActivityIdAsync(activity.Id, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetCategoriesByActivityId_EmptyId_ShouldThrowBadRequest()
    {
        var id = Guid.Empty;
        var userId = Guid.NewGuid();

        ActivityAccessServiceMock.SetupValidateAccessThrowsBadRequest(id, userId);

        await FluentActions
            .Awaiting(() => Service.GetCategoriesByActivityIdAsync(id, userId, CancellationToken.None))
            .Should()
            .ThrowAsync<BadRequestException>();

        ActivityAccessServiceMock.VerifyValidateAccessCalled(id, userId);
    }

    [Fact]
    public async Task GetCategoriesByActivityId_ActivityNotFound_ShouldThrowNotFound()
    {
        var id = Guid.NewGuid();
        var userId = Guid.NewGuid();

        ActivityAccessServiceMock.SetupValidateAccessThrowsNotFound(id, userId);

        await FluentActions
            .Awaiting(() => Service.GetCategoriesByActivityIdAsync(id, userId, CancellationToken.None))
            .Should()
            .ThrowAsync<NotFoundException>();

        ActivityAccessServiceMock.VerifyValidateAccessCalled(id, userId);
    }
}
