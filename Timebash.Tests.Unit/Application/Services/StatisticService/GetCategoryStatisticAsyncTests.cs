using FluentAssertions;
using Moq;
using Timebash.Core.Entities;
using Timebash.Core.Exceptions;

namespace Timebash.Tests.Unit.Application.Services.StatisticService;

public class GetCategoryStatisticAsyncTests : StatisticServiceTestsBase
{
    [Fact]
    public async Task GetCategoryStatistic_EmptyId_ShouldThrowBadRequest()
        => await FluentActions
            .Awaiting(() => Service.GetCategoryStatisticAsync(Guid.Empty, null, null, Guid.NewGuid()))
            .Should()
            .ThrowAsync<BadRequestException>();

    [Fact]
    public async Task GetCategoryStatistic_CategoryNotFound_ShouldThrowNotFound()
    {
        var id = Guid.NewGuid();
        CategoryRepositoryMock.Setup(repository => repository.GetByIdAsync(id)).ReturnsAsync((Category?)null);

        await FluentActions
            .Awaiting(() => Service.GetCategoryStatisticAsync(id, null, null, Guid.NewGuid()))
            .Should()
            .ThrowAsync<NotFoundException>();
    }
}
