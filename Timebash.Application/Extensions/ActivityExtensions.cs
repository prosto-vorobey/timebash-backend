using Timebash.Core.DTOs.Requests;
using Timebash.Core.DTOs.Responses;

namespace Timebash.Application.Extensions;

public static class ActivityExtensions
{
    public static ActivityResponse ToResponse(this Activity activity)
        => new(
            activity.Id,
            activity.JournalId,
            activity.Name,
            activity.StartTime,
            activity.EndTime,
            activity.CreatedAt,
            activity.UpdatedAt,
            activity.Duration
        );

    public static bool ApplyUpdate(this Activity activity, ActivityRequest request)
    {
        var result = false;

        if (activity.JournalId != request.JournalId)
        {
            activity.JournalId = request.JournalId;
            result = true;
        }
        if (activity.Name != request.Name)
        {
            activity.Name = request.Name;
            result = true;
        }
        if (activity.UpdateTimeRange(request.StartTime, request.EndTime)) result = true;

        return result;
    }
}
