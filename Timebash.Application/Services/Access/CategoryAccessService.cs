using Timebash.Core.Exceptions;
using Timebash.Core.Repositories;
using Timebash.Core.Services.Access;

namespace Timebash.Application.Services.Access;

public class CategoryAccessService(ICategoryRepository repository) : ICategoryAccessService
{
    private readonly ICategoryRepository _repository = repository;

    public async Task<Category> EnsureAccessAsync(Guid id, Guid userId, CancellationToken cancellationToken)
    {
        CheckInvalidId(id);

        var category = await _repository.GetByIdAsync(id, cancellationToken);
        if (category is null || category.UserId != userId) throw new NotFoundException();

        return category;
    }

    public async Task ValidateAccessAsync(Guid id, Guid userId, CancellationToken cancellationToken)
    {
        CheckInvalidId(id);
        if (!await _repository.IsUserLinkedAsync(id, userId, cancellationToken)) throw new NotFoundException();
    }

    private static void CheckInvalidId(Guid id)
    {
        if (id == Guid.Empty) throw new BadRequestException("Invalid id");
    }
}
