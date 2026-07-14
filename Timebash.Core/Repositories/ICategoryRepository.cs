namespace Timebash.Core.Repositories;

public interface ICategoryRepository : IRepository<Category>
{
    Task<IEnumerable<Category>> GetByIdsAsync(IEnumerable<Guid> categoryIds, CancellationToken cancellationToken);
    Task<IEnumerable<Category>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken);
    Task<IEnumerable<Activity>> GetActivitiesByCategoryIdAsync(Guid categoryId, CancellationToken cancellationToken);
    Task<bool> IsUserLinkedAsync(Guid categoryId, Guid userId, CancellationToken cancellationToken);
}
