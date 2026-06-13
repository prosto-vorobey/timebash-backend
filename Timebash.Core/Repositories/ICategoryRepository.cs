namespace Timebash.Core.Repositories;

public interface ICategoryRepository : IRepository<Category>
{
    Task<IEnumerable<Category>> GetByIdsAsync(IEnumerable<Guid> categoryIds);
    Task<IEnumerable<Category>> GetByUserIdAsync(Guid userId);
    Task<IEnumerable<Activity>> GetActivitiesByCategoryIdAsync(Guid categoryId);
    Task<bool> IsUserLinkedAsync(Guid categoryId, Guid userId);
}
