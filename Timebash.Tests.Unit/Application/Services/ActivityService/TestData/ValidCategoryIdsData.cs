using System.Collections;
using Bogus;

namespace Timebash.Tests.Unit.Application.Services.ActivityService.TestData;

public class ValidCategoryIdsData : IEnumerable<object[]>
{
    private static readonly Faker _faker = new();

    public IEnumerator<object[]> GetEnumerator()
    {
        var userId = Guid.NewGuid();
        var categoryIds = Enumerable.Range(0, _faker.Random.Number(2, 5))
            .Select(_ => Guid.NewGuid())
            .ToList();

        yield return new object[] { userId, categoryIds.Take(1).ToList() };
        yield return new object[] { userId, categoryIds };
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
