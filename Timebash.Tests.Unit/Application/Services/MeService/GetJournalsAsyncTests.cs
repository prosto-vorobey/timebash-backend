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

        SetupUserValidateExists(userId);
        JournalRepositoryMock.Setup(repository => repository.GetByUserIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(journals);

        var result = await Service.GetJournalsAsync(userId, CancellationToken.None);

        result.Should().BeEquivalentTo(expected);
        VerifyValidateExistsCalled(userId);
        JournalRepositoryMock.Verify(repository => repository.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetJournals_EmptyId_ShouldThrowBadRequest()
    {
        var id = Guid.Empty;
        SetupUserValidateExistsThrowsBadRequest(id);

        await FluentActions
            .Awaiting(() => Service.GetJournalsAsync(id, CancellationToken.None))
            .Should()
            .ThrowAsync<BadRequestException>();

        VerifyValidateExistsCalled(id);
        VerifyGetJournalsByUserIdNotCalled();
    }

    [Fact]
    public async Task GetJournals_UserNotFound_ShouldThrowNotFound()
    {
        var id = Guid.NewGuid();
        SetupUserValidateExistsThrowsNotFound(id);

        await FluentActions
            .Awaiting(() => Service.GetJournalsAsync(id, CancellationToken.None))
            .Should()
            .ThrowAsync<NotFoundException>();

        VerifyValidateExistsCalled(id);
        VerifyGetJournalsByUserIdNotCalled();
    }

    private void VerifyGetJournalsByUserIdNotCalled()
        => JournalRepositoryMock.Verify(repository => repository.GetByUserIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
}
