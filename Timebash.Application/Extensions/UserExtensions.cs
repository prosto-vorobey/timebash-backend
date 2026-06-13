using Timebash.Core.DTOs.Responses;

namespace Timebash.Application.Extensions;

public static class UserExtensions
{
    public static UserResponse ToResponse(this User user)
        => new(
            user.Id,
            user.Name,
            user.Email,
            user.CreatedAt
        );
}
