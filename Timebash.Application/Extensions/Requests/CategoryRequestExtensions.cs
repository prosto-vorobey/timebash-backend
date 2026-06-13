using Timebash.Core.DTOs.Requests;

namespace Timebash.Application.Extensions.Requests;

public static class CategoryRequestExtensions
{
    public static Category ToCategory(this CategoryRequest request, Guid id, Guid userId)
        => new(id, userId, request.Name, request.Color)
        {
            Keywords = request.Keywords ?? [],
        };
}
