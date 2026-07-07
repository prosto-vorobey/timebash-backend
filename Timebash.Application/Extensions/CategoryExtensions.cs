using Timebash.Core.DTOs.Requests;
using Timebash.Core.DTOs.Responses;

namespace Timebash.Application.Extensions;

public static class CategoryExtensions
{
    public static CategoryResponse ToResponse(this Category category)
        => new(
            category.Id,
            category.UserId,
            category.Name,
            category.Color,
            category.Keywords,
            category.CreatedAt,
            category.UpdatedAt
        );

    public static bool ApplyUpdate(this Category category, CategoryRequest request)
    {
        bool result = false;
        if (request.Name != category.Name)
        {
            category.Name = request.Name;
            result = true;
        }
        if (request.Color != category.Color)
        {
            category.Color = request.Color;
            result = true;
        }
        if (request.Keywords != null && !request.Keywords.ToHashSet().SetEquals(category.Keywords))
        {
            category.Keywords = [.. request.Keywords];
            result = true;
        }
        return result;
    }
}
