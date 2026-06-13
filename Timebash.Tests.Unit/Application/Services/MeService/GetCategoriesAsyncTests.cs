using FluentAssertions;
using Moq;
using Timebash.Application.Extensions;
using Timebash.Core.DTOs.Responses;
using Timebash.Core.Entities;

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

        CategoryRepositoryMock.Setup(repository => repository.GetByUserIdAsync(userId)).ReturnsAsync(categories);

        var result = await Service.GetCategoriesAsync(userId);
        result.Should().BeEquivalentTo(expected);
    }
}
