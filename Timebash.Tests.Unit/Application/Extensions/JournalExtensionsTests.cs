using Bogus;
using Timebash.Core.Entities;
using Timebash.Application.Extensions;
using FluentAssertions;
using Timebash.Core.DTOs.Requests;

namespace Timebash.Tests.Unit.Application.Extensions;

public class JournalExtensionsTests
{
    private static readonly Faker _faker = new();

    [Fact]
    public void ToResponse_ShouldMapAllPropertiesCorrectly()
    {
        var journal = new Journal(Guid.NewGuid(), Guid.NewGuid(), _faker.Lorem.Word());

        var result = journal.ToResponse();

        result.Id.Should().Be(journal.Id);
        result.UserId.Should().Be(journal.UserId);
        result.Name.Should().Be(journal.Name);
        result.CreatedAt.Should().Be(journal.CreatedAt);
        result.UpdatedAt.Should().Be(journal.UpdatedAt);
    }

    [Fact]
    public void ApplyUpdate_WhenNameChanged_ShouldReturnTrueAndUpdate()
    {
        var id = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var journal = new Journal(id, userId, _faker.Lorem.Word());
        var currentCreatedTime = journal.CreatedAt;
        var currentUpdatedTime = journal.UpdatedAt;
        var newName = $"{journal.Name} changed";
        var request = new JournalRequest(newName);

        journal.ApplyUpdate(request).Should().BeTrue();
        AssertJournalFields(journal, id, userId, newName, currentCreatedTime, currentUpdatedTime);
    }

    [Fact]
    public void ApplyUpdate_NoChanges_ShouldReturnFalse()
    {
        var id = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var name = _faker.Lorem.Word();
        var journal = new Journal(id, userId, name);
        var currentCreatedTime = journal.CreatedAt;
        var currentUpdatedTime = journal.UpdatedAt;
        var request = new JournalRequest(name);

        journal.ApplyUpdate(request).Should().BeFalse();
        AssertJournalFields(journal, id, userId, name, currentCreatedTime, currentUpdatedTime);
    }
    
    private static void AssertJournalFields(Journal journal, Guid id, Guid userId, string name, DateTime createdAt, DateTime updatedAt)
    {
        journal.Id.Should().Be(id);
        journal.UserId.Should().Be(userId);
        journal.Name.Should().Be(name);
        journal.CreatedAt.Should().Be(createdAt);
        journal.UpdatedAt.Should().Be(updatedAt);
    }
}
