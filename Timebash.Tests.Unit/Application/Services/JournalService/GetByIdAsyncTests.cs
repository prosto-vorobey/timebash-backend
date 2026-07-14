using FluentAssertions;
using Moq;
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

        JournalRepositoryMock.Setup(repository => repository.GetByIdAsync(journal.Id, It.IsAny<CancellationToken>())).ReturnsAsync(journal);

        var result = await Service.GetByIdAsync(journal.Id, journal.UserId, CancellationToken.None);
        result.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public async Task GetById_EmptyId_ShouldThrowBadRequest()
        => await FluentActions
            .Awaiting(() => Service.GetByIdAsync(Guid.Empty, Guid.NewGuid(), CancellationToken.None))
            .Should()
            .ThrowAsync<BadRequestException>();

    [Fact]
    public async Task GetById_JournalNotFound_ShouldThrowNotFound()
    {
        var id = Guid.NewGuid();
        JournalRepositoryMock.Setup(repository => repository.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync((Journal?)null);

        await FluentActions
            .Awaiting(() => Service.GetByIdAsync(id, Guid.NewGuid(), CancellationToken.None))
            .Should()
            .ThrowAsync<NotFoundException>();
    }
}
