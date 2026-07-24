using FluentAssertions;
using Moq;
using Timebash.Application.Extensions;
using Timebash.Core.DTOs.Responses;
using Timebash.Core.Entities;
using Timebash.Core.Exceptions;
using Timebash.Tests.Unit.TestInfrastructure.MockExtensions.AccessServices;

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

        AccessServiceMock.SetupValidateAccess(category.Id, category.UserId);
        RepositoryMock
            .Setup(repository => repository.GetActivitiesByCategoryIdAsync(category.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(activities);

        var result = await Service.GetActivitiesByCategoryIdAsync(category.Id, category.UserId, CancellationToken.None);
        
        result.Should().BeEquivalentTo(expected);
        AccessServiceMock.VerifyValidateAccessCalled(category.Id, category.UserId);
        RepositoryMock.Verify(repository => repository.GetActivitiesByCategoryIdAsync(category.Id, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetActivitiesByCategoryId_EmptyId_ShouldThrowBadRequest()
    {
        var id = Guid.Empty;
        var userId = Guid.NewGuid();

        AccessServiceMock.SetupValidateAccessThrowsBadRequest(id, userId);

        await FluentActions
            .Awaiting(() => Service.GetActivitiesByCategoryIdAsync(id, userId, CancellationToken.None))
            .Should()
            .ThrowAsync<BadRequestException>();

        AccessServiceMock.VerifyValidateAccessCalled(id, userId);
    }

    [Fact]
    public async Task GetActivitiesByCategoryId_CategoryNotFound_ShouldThrowNotFound()
    {
        var id = Guid.NewGuid();
        var userId = Guid.NewGuid();

        AccessServiceMock.SetupValidateAccessThrowsNotFound(id, userId);

        await FluentActions
            .Awaiting(() => Service.GetActivitiesByCategoryIdAsync(id, userId, CancellationToken.None))
            .Should()
            .ThrowAsync<NotFoundException>();

        AccessServiceMock.VerifyValidateAccessCalled(id, userId);
    }
}
