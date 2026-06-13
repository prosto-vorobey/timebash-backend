using Bogus;
using FluentAssertions;
using Timebash.Application.Extensions.Requests;
using Timebash.Core.DTOs.Requests;

namespace Timebash.Tests.Unit.Application.Extensions.Requests;

public class JournalRequestExtensionsTests
{
    private static readonly Faker _faker = new();

    [Fact]
    public void ToJournal_ShouldMapAllPropertiesCorrectly()
    {
        var id = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var request = new JournalRequest(_faker.Lorem.Word());

        var result = request.ToJournal(id, userId);

        result.Id.Should().Be(id);
        result.UserId.Should().Be(userId);
        result.Name.Should().Be(request.Name);
    }
}
