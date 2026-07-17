using FluentAssertions;
using Moq;
using Timebash.Application.Extensions;
using Timebash.Application.Extensions.Requests;
using Timebash.Core.DTOs.Requests;
using Timebash.Core.Entities;

namespace Timebash.Tests.Unit.Application.Services.JournalService;

public class CreateAsyncTests : JournalServiceTestsBase
{
    [Fact]
    public async Task Create_ShouldReturnResponse()
    {
        var request = new JournalRequest(Faker.Lorem.Word());
        var userId = Guid.NewGuid();

        var capturedJournals = new List<Journal>();
        JournalRepositoryMock.Setup(repository => repository.Add(It.IsAny<Journal>())).Callback(capturedJournals.Add);

        var result = await Service.CreateAsync(request, userId, CancellationToken.None);

        capturedJournals.Should().HaveCount(1);

        var capturedJournal = capturedJournals.First();
        capturedJournal.Id.Should().NotBeEmpty();
        capturedJournal.Should().BeEquivalentTo(
            request.ToJournal(capturedJournal.Id, userId),
            options => options
                .Excluding(journal => journal.CreatedAt)
                .Excluding(journal => journal.UpdatedAt));

        result.Should().BeEquivalentTo(capturedJournal.ToResponse());
        VerifySaveChangesCalled();
    }
}
