using FluentAssertions;
using Timebash.Application.Extensions;
using Timebash.Core.DTOs.Requests;
using Timebash.Core.Entities;
using Timebash.Core.Exceptions;
using Timebash.Tests.Unit.TestInfrastructure.MockExtensions;
using Timebash.Tests.Unit.TestInfrastructure.MockExtensions.AccessServices;

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

        AccessServiceMock.SetupEnsureAccess(journal);
        UnitOfWorkMock.SetupSaveChanges();

        var result = await Service.UpdateAsync(journal.Id, request, journal.UserId, CancellationToken.None);

        result.Should().BeTrue();
        journal.UpdatedAt.Should().BeAfter(currentUpdatedTime);
        journal.Should().BeEquivalentTo(
            expected,
            options => options
                .Excluding(journal => journal.CreatedAt)
                .Excluding(journal => journal.UpdatedAt));

        AccessServiceMock.VerifyEnsureAccessCalled(journal.Id, journal.UserId);
        UnitOfWorkMock.VerifySaveChangesCalled();
    }

    [Fact]
    public async Task Update_NoChanges_ShouldReturnFalse()
    {
        var journal = new Journal(Guid.NewGuid(), Guid.NewGuid(), Faker.Lorem.Word());
        var request = new JournalRequest(journal.Name);

        var currentUpdateTime = journal.UpdatedAt;
        var expected = new Journal(journal.Id, journal.UserId, journal.Name);

        AccessServiceMock.SetupEnsureAccess(journal);

        var result = await Service.UpdateAsync(journal.Id, request, journal.UserId, CancellationToken.None);

        result.Should().BeFalse();
        journal.UpdatedAt.Should().Be(currentUpdateTime);
        journal.Should().BeEquivalentTo(
            expected,
            options => options
                .Excluding(journal => journal.CreatedAt)
                .Excluding(journal => journal.UpdatedAt));

        AccessServiceMock.VerifyEnsureAccessCalled(journal.Id, journal.UserId);
    }

    [Fact]
    public async Task Update_EmptyId_ShouldThrowBadRequest()
    {
        var id = Guid.Empty;
        var userId = Guid.NewGuid();

        AccessServiceMock.SetupEnsureAccessThrowsBadRequest(id, userId);

        await FluentActions
            .Awaiting(() => Service.UpdateAsync(id, new JournalRequest(Faker.Lorem.Word()), userId, CancellationToken.None))
            .Should()
            .ThrowAsync<BadRequestException>();

        AccessServiceMock.VerifyEnsureAccessCalled(id, userId);
    }

    [Fact]
    public async Task Update_JournalNotFound_ShouldThrowNotFound()
    {
        var id = Guid.NewGuid();
        var userId = Guid.NewGuid();

        AccessServiceMock.SetupEnsureAccessThrowsNotFound(id, userId);

        await FluentActions
            .Awaiting(() => Service.UpdateAsync(id, new JournalRequest(Faker.Lorem.Word()), userId, CancellationToken.None))
            .Should()
            .ThrowAsync<NotFoundException>();

        AccessServiceMock.VerifyEnsureAccessCalled(id, userId);
    }
}
