using System.Collections;

namespace Timebash.Tests.Unit.Application.Services.ActivityService.TestData;

public class EmptyCategoryIdsData : IEnumerable<object[]>
{
    public IEnumerator<object[]> GetEnumerator()
    {
        yield return new object[] { Guid.NewGuid(), new List<Guid>() };
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
