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

        ActivityAccessServiceMock
            .Setup(service => service.ValidateAccessAsync(activity.Id, userId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        ActivityRepositoryMock
            .Setup(repository => repository.GetCategoriesByActivityIdAsync(activity.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(categories);

        var result = await Service.GetCategoriesByActivityIdAsync(activity.Id, userId, CancellationToken.None);

        result.Should().BeEquivalentTo(expected, options => options.WithoutStrictOrdering());
        VerifyActivityValidateAccessCalled(activity.Id, userId);
        ActivityRepositoryMock.Verify(repository => repository.GetCategoriesByActivityIdAsync(activity.Id, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetCategoriesByActivityId_EmptyId_ShouldThrowBadRequest()
    {
        var id = Guid.Empty;
        var userId = Guid.NewGuid();

        ActivityAccessServiceMock
            .Setup(service => service.ValidateAccessAsync(id, userId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new BadRequestException());

        await FluentActions
            .Awaiting(() => Service.GetCategoriesByActivityIdAsync(id, userId, CancellationToken.None))
            .Should()
            .ThrowAsync<BadRequestException>();

        VerifyActivityValidateAccessCalled(id, userId);
        VerifyActivityGetCategoriesByActivityIdNotCalled();
    }

    [Fact]
    public async Task GetCategoriesByActivityId_ActivityNotFound_ShouldThrowNotFound()
    {
        var id = Guid.NewGuid();
        var userId = Guid.NewGuid();

        ActivityAccessServiceMock
            .Setup(service => service.ValidateAccessAsync(id, userId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException());

        await FluentActions
            .Awaiting(() => Service.GetCategoriesByActivityIdAsync(id, userId, CancellationToken.None))
            .Should()
            .ThrowAsync<NotFoundException>();

        VerifyActivityValidateAccessCalled(id, userId);
        VerifyActivityGetCategoriesByActivityIdNotCalled();
    }

    private void VerifyActivityValidateAccessCalled(Guid id, Guid userId)
        => ActivityAccessServiceMock.Verify(service => service.ValidateAccessAsync(id, userId, It.IsAny<CancellationToken>()), Times.Once);

    private void VerifyActivityGetCategoriesByActivityIdNotCalled()
        => ActivityRepositoryMock.Verify(
            repository => repository.GetCategoriesByActivityIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);

}
