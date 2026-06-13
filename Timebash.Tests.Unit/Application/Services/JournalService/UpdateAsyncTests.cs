using FluentAssertions;
using Moq;
using Timebash.Application.Extensions;
using Timebash.Core.DTOs.Requests;
using Timebash.Core.Entities;
using Timebash.Core.Exceptions;

namespace Timebash.Tests.Unit.Application.Services.JournalService;

public class UpdateAsyncTests : JournalServiceTestsBase
{
    [Fact]
    public async Task Update_ValidRequest_ShouldReturnTrueAndUpdate()
    {
        var journal = new Journal(Guid.NewGuid(), Guid.NewGuid(), Faker.Lorem.Word());
        var request = new JournalRequest($"{journal.Name} changed");

        var currentUpdatedTime = journal.UpdatedAt;
        var expected = new Journal(journal.Id, journal.UserId, journal.Name);
        expected.ApplyUpdate(request);

        JournalRepositoryMock.Setup(repository => repository.GetByIdAsync(journal.Id)).ReturnsAsync(journal);

        var result = await Service.UpdateAsync(journal.Id, request, journal.UserId);

        result.Should().BeTrue();
        journal.UpdatedAt.Should().BeAfter(currentUpdatedTime);
        journal.Should().BeEquivalentTo(
            expected,
            options => options
                .Excluding(journal => journal.CreatedAt)
                .Excluding(journal => journal.UpdatedAt));

        UnitOfWorkMock.Verify(unit => unit.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task Update_NoChanges_ShouldReturnFalse()
    {
        var journal = new Journal(Guid.NewGuid(), Guid.NewGuid(), Faker.Lorem.Word());
        var request = new JournalRequest(journal.Name);

        var currentUpdateTime = journal.UpdatedAt;
        var expected = new Journal(journal.Id, journal.UserId, journal.Name);

        JournalRepositoryMock.Setup(repository => repository.GetByIdAsync(journal.Id)).ReturnsAsync(journal);

        var result = await Service.UpdateAsync(journal.Id, request, journal.UserId);

        result.Should().BeFalse();
        journal.UpdatedAt.Should().Be(currentUpdateTime);
        journal.Should().BeEquivalentTo(
            expected,
            options => options
                .Excluding(journal => journal.CreatedAt)
                .Excluding(journal => journal.UpdatedAt));

        UnitOfWorkMock.Verify(unit => unit.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task Update_EmptyId_ShouldThrowBadRequest()
        => await FluentActions
            .Awaiting(() => Service.UpdateAsync(Guid.Empty, new JournalRequest(Faker.Lorem.Word()), Guid.NewGuid()))
            .Should()
            .ThrowAsync<BadRequestException>();

    [Fact]
    public async Task Update_JournalNotFound_ShouldThrowNotFound()
    {
        var id = Guid.NewGuid();
        JournalRepositoryMock.Setup(repository => repository.GetByIdAsync(id)).ReturnsAsync((Journal?)null);

        await FluentActions
            .Awaiting(() => Service.UpdateAsync(id, new JournalRequest(Faker.Lorem.Word()), Guid.NewGuid()))
            .Should()
            .ThrowAsync<NotFoundException>();
    }
}
