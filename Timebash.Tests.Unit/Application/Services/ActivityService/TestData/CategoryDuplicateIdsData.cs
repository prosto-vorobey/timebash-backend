using System.Collections;
using Bogus;
using Timebash.Core.Entities;

namespace Timebash.Tests.Unit.Application.Services.ActivityService.TestData;

public class CategoryDuplicateIdsData : IEnumerable<object[]>
{
    private static readonly Faker _faker = new();

    public IEnumerator<object[]> GetEnumerator()
    {
        var userId = Guid.NewGuid();
        var categoryIds = Enumerable.Range(0, _faker.Random.Number(2, 5))
            .Select(_ => Guid.NewGuid())
            .ToList();

        var categories = categoryIds.Select(id => new Category(id, userId, _faker.Lorem.Word(), "#000000")).ToList();

        yield return new object[] { userId, categoryIds.Concat([.. categoryIds]).ToList(), categories };
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
