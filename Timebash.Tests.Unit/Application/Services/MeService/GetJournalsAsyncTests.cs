using FluentAssertions;
using Moq;
using Timebash.Application.Extensions;
using Timebash.Core.DTOs.Responses;
using Timebash.Core.Entities;
using Timebash.Core.Exceptions;
using Timebash.Tests.Unit.TestInfrastructure.MockExtensions.AccessServices;

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

        UserAccessServiceMock.SetupValidateExists(userId);
        JournalRepositoryMock.Setup(repository => repository.GetByUserIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(journals);

        var result = await Service.GetJournalsAsync(userId, CancellationToken.None);

        result.Should().BeEquivalentTo(expected);
        UserAccessServiceMock.VerifyValidateExistsCalled(userId);
        JournalRepositoryMock.Verify(repository => repository.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetJournals_EmptyId_ShouldThrowBadRequest()
    {
        var id = Guid.Empty;
        UserAccessServiceMock.SetupValidateExistsThrowsBadRequest(id);

        await FluentActions
            .Awaiting(() => Service.GetJournalsAsync(id, CancellationToken.None))
            .Should()
            .ThrowAsync<BadRequestException>();

        UserAccessServiceMock.VerifyValidateExistsCalled(id);
    }

    [Fact]
    public async Task GetJournals_UserNotFound_ShouldThrowNotFound()
    {
        var id = Guid.NewGuid();
        UserAccessServiceMock.SetupValidateExistsThrowsNotFound(id);

        await FluentActions
            .Awaiting(() => Service.GetJournalsAsync(id, CancellationToken.None))
            .Should()
            .ThrowAsync<NotFoundException>();

        UserAccessServiceMock.VerifyValidateExistsCalled(id);
    }
}
