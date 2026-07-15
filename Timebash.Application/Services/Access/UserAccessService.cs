using Timebash.Core.Exceptions;
using Timebash.Core.Repositories;
using Timebash.Core.Services.Access;

namespace Timebash.Application.Services.Access;

public class UserAccessService(IUserRepository repository) : IUserAccessService
{
    private readonly IUserRepository _repository = repository;

    public async Task<User> EnsureAccessAsync(Guid id, CancellationToken cancellationToken)
    {
        CheckInvalidId(id);
        var user = await _repository.GetByIdAsync(id, cancellationToken) ?? throw new NotFoundException();

        return user;
    }

    public async Task ValidateExistsAsync(Guid id, CancellationToken cancellationToken)
    {
        CheckInvalidId(id);
        if (!await _repository.ExistsAsync(id, cancellationToken)) throw new NotFoundException();
    }

    private static void CheckInvalidId(Guid id)
    {
        if (id == Guid.Empty) throw new BadRequestException("Invalid id");
    }
}
