using Timebash.Core.DTOs.Requests;

namespace Timebash.Application.Extensions.Requests;

public static class JournalRequestExtensions
{
    public static Journal ToJournal(this JournalRequest request, Guid id, Guid userId)
        => new(id, userId, request.Name);
}
