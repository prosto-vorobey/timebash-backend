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

        SetupActivityEnsureAccess(activity, userId);

        var result = await Service.GetByIdAsync(activity.Id, userId, CancellationToken.None);

        result.Should().BeEquivalentTo(expected);
        VerifyActivityEnsureAccessCalled(activity.Id, userId);
    }

    [Fact]
    public async Task GetById_EmptyId_ShouldThrowBadRequest()
    {
        var id = Guid.Empty;
        var userId = Guid.NewGuid();

        SetupActivityEnsureAccessThrowsBadRequest(id, userId);

        await FluentActions
            .Awaiting(() => Service.GetByIdAsync(id, userId, CancellationToken.None))
            .Should()
            .ThrowAsync<BadRequestException>();

        VerifyActivityEnsureAccessCalled(id, userId);
    }

    [Fact]
    public async Task GetById_ActivityNotFound_ShouldThrowNotFound()
    {
        var id = Guid.NewGuid();
        var userId = Guid.NewGuid();

        SetupActivityEnsureAccessThrowsNotFound(id, userId);

        await FluentActions
            .Awaiting(() => Service.GetByIdAsync(id, userId, CancellationToken.None))
            .Should()
            .ThrowAsync<NotFoundException>();
        
        VerifyActivityEnsureAccessCalled(id, userId);
    }
}
