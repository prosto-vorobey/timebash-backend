using System.Collections;
using Bogus;

namespace Timebash.Tests.Unit.Application.Services.ActivityService.TestData;

public class EmptyOrEmptyGuidListData : IEnumerable<object[]>
{
    private static readonly Faker _faker = new();

    public IEnumerator<object[]> GetEnumerator()
    {
        var emptyCategoryIds = Enumerable.Range(0, _faker.Random.Number(1, 5))
            .Select(_ => Guid.Empty)
            .ToList();

        yield return new object[] { new List<Guid>() };
        yield return new object[] { emptyCategoryIds };

    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
