using FluentAssertions;
using Moq;
using Timebash.Application.Extensions;
using Timebash.Core.DTOs.Responses;
using Timebash.Core.Entities;

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

        JournalRepositoryMock.Setup(repository => repository.GetByUserIdAsync(userId)).ReturnsAsync(journals);

        var result = await Service.GetJournalsAsync(userId);
        result.Should().BeEquivalentTo(expected);
    }
}
