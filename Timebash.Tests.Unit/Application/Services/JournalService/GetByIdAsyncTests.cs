using FluentAssertions;
using Timebash.Application.Extensions;
using Timebash.Core.Entities;
using Timebash.Core.Exceptions;

namespace Timebash.Tests.Unit.Application.Services.JournalService;

public class GetByIdAsyncTests : JournalServiceTestsBase
{
    [Fact]
    public async Task GetByAsync_ValidAccess_ShouldReturnResponse()
    {
        var journal = new Journal(Guid.NewGuid(), Guid.NewGuid(), Faker.Lorem.Word());
        var expected = journal.ToResponse();

        AccessServiceMock.SetupEnsureAccess(journal);

        var result = await Service.GetByIdAsync(journal.Id, journal.UserId, CancellationToken.None);
        
        result.Should().BeEquivalentTo(expected);
        AccessServiceMock.VerifyEnsureAccessCalled(journal.Id, journal.UserId);
    }

    [Fact]
    public async Task GetById_EmptyId_ShouldThrowBadRequest()
    {
        var id = Guid.Empty;
        var userId = Guid.NewGuid();

        AccessServiceMock.SetupEnsureAccessThrowsBadRequest(id, userId);

        await FluentActions
            .Awaiting(() => Service.GetByIdAsync(id, userId, CancellationToken.None))
            .Should()
            .ThrowAsync<BadRequestException>();

        AccessServiceMock.VerifyEnsureAccessCalled(id, userId);
    }

    [Fact]
    public async Task GetById_JournalNotFound_ShouldThrowNotFound()
    {
        var id = Guid.NewGuid();
        var userId = Guid.NewGuid();

        AccessServiceMock.SetupEnsureAccessThrowsNotFound(id, userId);

        await FluentActions
            .Awaiting(() => Service.GetByIdAsync(id, userId, CancellationToken.None))
            .Should()
            .ThrowAsync<NotFoundException>();

        AccessServiceMock.VerifyEnsureAccessCalled(id, userId);
    }
}
