using Timebash.Core.DTOs.Requests;

namespace Timebash.Application.Extensions.Requests;

public static class RegisterRequestExtensions
{
    public static User ToUser(this RegisterRequest request, Guid id)
    => new(id, request.Name, request.Email);
}
