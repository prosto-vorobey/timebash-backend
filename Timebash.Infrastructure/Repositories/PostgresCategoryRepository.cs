using Microsoft.EntityFrameworkCore;

namespace Timebash.Infrastructure.Repositories;

public class PostgresCategoryRepository(TimebashDbContext context) : PostgresRepositoryBase<Category>(context), ICategoryRepository
{
    public async Task<IEnumerable<Category>> GetByIdsAsync(IEnumerable<Guid> categoryIds, CancellationToken cancellationToken)
    {
        if (categoryIds == null || !categoryIds.Any()) return [];

        return await Context.Categories
            .Where(category => categoryIds.Contains(category.Id))
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Category>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken)
        => await Context.Categories
            .Where(category => category.UserId == userId)
            .ToListAsync(cancellationToken);

    public async Task<IEnumerable<Activity>> GetActivitiesByCategoryIdAsync(Guid categoryId, CancellationToken cancellationToken)
        => await Context.ActivityCategories
            .Where(pair => pair.CategoryId == categoryId)
            .Select(pair => pair.Activity)
            .ToListAsync(cancellationToken);

    public async Task<bool> IsUserLinkedAsync(Guid categoryId, Guid userId, CancellationToken cancellationToken)
        => await Context.Journals.AnyAsync(category => category.Id == categoryId && category.User.Id == userId, cancellationToken);
}
