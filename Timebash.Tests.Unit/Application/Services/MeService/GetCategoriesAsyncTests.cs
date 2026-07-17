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

        SetupUserValidateExists(userId);
        CategoryRepositoryMock.Setup(repository => repository.GetByUserIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(categories);

        var result = await Service.GetCategoriesAsync(userId, CancellationToken.None);

        result.Should().BeEquivalentTo(expected);
        VerifyValidateExistsCalled(userId);
        CategoryRepositoryMock.Verify(repository => repository.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetCategories_EmptyId_ShouldThrowBadRequest()
    {
        var id = Guid.Empty;
        SetupUserValidateExistsThrowsBadRequest(id);

        await FluentActions
            .Awaiting(() => Service.GetCategoriesAsync(id, CancellationToken.None))
            .Should()
            .ThrowAsync<BadRequestException>();

        VerifyValidateExistsCalled(id);
        VerifyGetCategoriesByUserIdNotCalled();
    }

    [Fact]
    public async Task GetCategories_UserNotFound_ShouldThrowNotFound()
    {
        var id = Guid.NewGuid();
        SetupUserValidateExistsThrowsNotFound(id);

        await FluentActions
            .Awaiting(() => Service.GetCategoriesAsync(id, CancellationToken.None))
            .Should()
            .ThrowAsync<NotFoundException>();

        VerifyValidateExistsCalled(id);
        VerifyGetCategoriesByUserIdNotCalled();
    }

    private void VerifyGetCategoriesByUserIdNotCalled()
        => CategoryRepositoryMock.Verify(repository => repository.GetByUserIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
}
