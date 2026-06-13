using System.Collections;
using Bogus;
using Moq;
using Timebash.Core.Entities;

namespace Timebash.Tests.Unit.Application.Services.ActivityService.TestData;

public class CategoriesWithEmptyGuidsData : IEnumerable<object[]>
{
    private static readonly Faker _faker = new();

    public IEnumerator<object[]> GetEnumerator()
    {
        var userId = Guid.NewGuid();
        var categoryIds = Enumerable.Range(0, _faker.Random.Number(2, 5))
            .Select(_ => Guid.NewGuid())
            .ToList();

        var categories = categoryIds.Select(id => new Category(id, userId, _faker.Lorem.Word(), "#000000")).ToList();

        var emptyCategoryIds = Enumerable.Range(0, _faker.Random.Number(1, 5))
            .Select(_ => Guid.Empty)
            .ToList();

        yield return new object[] { userId, categoryIds.Append(Guid.Empty).ToList(), categories };
        yield return new object[] { userId, categoryIds.Concat(emptyCategoryIds).ToList(), categories };
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
