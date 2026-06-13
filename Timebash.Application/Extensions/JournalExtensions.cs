using Timebash.Core.DTOs.Requests;
using Timebash.Core.DTOs.Responses;

namespace Timebash.Application.Extensions;

public static class JournalExtensions
{
    public static JournalResponse ToResponse(this Journal journal)
        => new(
            journal.Id,
            journal.UserId,
            journal.Name,
            journal.CreatedAt,
            journal.UpdatedAt
        );

    public static bool ApplyUpdate(this Journal journal, JournalRequest request)
    {
        var result = false;

        if (journal.Name != request.Name)
        {
            journal.Name = request.Name;
            result = true;
        }

        return result;
    }
}
