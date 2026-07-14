using FluentAssertions;
using Moq;
using Timebash.Application.Extensions;
using Timebash.Core.DTOs.Responses;
using Timebash.Core.Entities;
using Timebash.Core.Exceptions;

namespace Timebash.Tests.Unit.Application.Services.MeService;

public class GetJournalsAsyncTests : MeServiceTestsBase
{
    [Fact]
    public async Task GetJournals_ShouldReturnResponse()
    {
        var userId = Guid.NewGuid();
        var journals = new List<Journal>
        {
            new(Guid.NewGuid(), userId, Faker.Lorem.Word())
        };
        var expected = new JournalsListResponse([.. journals.Select(journal => journal.ToResponse())]);

        UserRepositoryMock.Setup(repository => repository.ExistsAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        JournalRepositoryMock.Setup(repository => repository.GetByUserIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(journals);

        var result = await Service.GetJournalsAsync(userId, CancellationToken.None);
        result.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public async Task GetJournals_EmptyId_ShouldThrowBadRequest()
        => await FluentActions
            .Awaiting(() => Service.GetJournalsAsync(Guid.Empty, CancellationToken.None))
            .Should()
            .ThrowAsync<BadRequestException>()
            .WithMessage("Invalid id");

    [Fact]
    public async Task GetJournals_UserNotFound_ShouldThrowNotFound()
    {
        var id = Guid.NewGuid();
        UserRepositoryMock.Setup(repository => repository.ExistsAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(false);

        await FluentActions
            .Awaiting(() => Service.GetJournalsAsync(id, CancellationToken.None))
            .Should()
            .ThrowAsync<NotFoundException>();
    }
}
