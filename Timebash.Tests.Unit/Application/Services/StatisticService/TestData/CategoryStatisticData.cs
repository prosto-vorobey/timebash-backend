using System.Collections;
using Timebash.Core.Entities;

namespace Timebash.Tests.Unit.Application.Services.StatisticService.TestData;

public class CategoryStatisticData : IEnumerable<object[]>
{
    private static readonly long durationSecond = 86_400L;

    public IEnumerator<object[]> GetEnumerator()
    {
        yield return PrependData(CategoryStatisticScenarioBuilder.GetDataWithoutActivities());
        yield return PrependData(CategoryStatisticScenarioBuilder.GetDataWithZeroDurationActivity());
        yield return PrependData(CategoryStatisticScenarioBuilder.GetDataWithSingleActivity(durationSecond));
        yield return PrependData(CategoryStatisticScenarioBuilder.GetDataWithActivityHavingMultipleCategories(durationSecond));
        yield return PrependData(CategoryStatisticScenarioBuilder.GetDataWithSomeActivities(durationSecond));
    
        object[] PrependData((Category Category, List<Activity> Activities, long ExpectedTime) data) 
            => [data.Category, data.Activities, data.ExpectedTime];
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
