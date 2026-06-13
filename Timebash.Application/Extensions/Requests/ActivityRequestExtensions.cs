using Timebash.Core.DTOs.Requests;

namespace Timebash.Application.Extensions.Requests;

public static class ActivityRequestExtensions
{
    public static Activity ToActivity(this ActivityRequest request, Guid id)
        => new(id, request.JournalId, request.StartTime, request.EndTime, request.Name);

    public static Activity ToActivity(this ActivityWithCorrectionRequest request, Guid id)
        => new(id, request.JournalId, request.StartTime, request.EndTime, request.Name);
}
