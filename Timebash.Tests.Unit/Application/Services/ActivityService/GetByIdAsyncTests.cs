using FluentAssertions;
using Moq;
using Timebash.Application.Extensions;
using Timebash.Core.Entities;
using Timebash.Core.Exceptions;

namespace Timebash.Tests.Unit.Application.Services.ActivityService;

public class GetByIdAsyncTests : ActivityServiceTestsBase
{
    [Fact]
    public async Task GetById_ValidAccess_ShouldReturnResponse()
    {
        var activity = new Activity(Guid.NewGuid(), Guid.NewGuid(), DateTime.MinValue, DateTime.MaxValue);
        var userId = Guid.NewGuid();
        var expected = activity.ToResponse();

        ActivityRepositoryMock.Setup(repository => repository.GetByIdAsync(activity.Id)).ReturnsAsync(activity);
        JournalRepositoryMock.Setup(repository => repository.IsUserLinkedAsync(activity.JournalId, userId)).ReturnsAsync(true);

        var result = await Service.GetByIdAsync(activity.Id, userId);

        result.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public async Task GetById_EmptyId_ShouldThrowBadRequest()
        => await FluentActions
            .Awaiting(() => Service.GetByIdAsync(Guid.Empty, Guid.NewGuid()))
            .Should()
            .ThrowAsync<BadRequestException>();

    [Fact]
    public async Task GetById_ActivityNotFound_ShouldThrowNotFound()
    {
        var id = Guid.NewGuid();
        ActivityRepositoryMock.Setup(repository => repository.GetByIdAsync(id)).ReturnsAsync((Activity?)null);

        await FluentActions
            .Awaiting(() => Service.GetByIdAsync(id, Guid.NewGuid()))
            .Should()
            .ThrowAsync<NotFoundException>();
    }
}
