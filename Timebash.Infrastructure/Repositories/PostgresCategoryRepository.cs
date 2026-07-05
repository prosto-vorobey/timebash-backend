using Microsoft.EntityFrameworkCore;

namespace Timebash.Infrastructure.Repositories;

public class PostgresCategoryRepository(TimebashDbContext context) : PostgresRepositoryBase<Category>(context), ICategoryRepository
{
    public async Task<IEnumerable<Category>> GetByIdsAsync(IEnumerable<Guid> categoryIds)
    {
        if (categoryIds == null || !categoryIds.Any()) return [];

        return await Context.Categories
            .Where(category => categoryIds.Contains(category.Id))
            .ToListAsync();
    }

    public async Task<IEnumerable<Category>> GetByUserIdAsync(Guid userId)
        => await Context.Categories
            .Where(category => category.UserId == userId)
            .ToListAsync();

    public async Task<IEnumerable<Activity>> GetActivitiesByCategoryIdAsync(Guid categoryId)
        => await Context.ActivityCategories
            .Where(pair => pair.CategoryId == categoryId)
            .Select(pair => pair.Activity)
            .ToListAsync();

    public async Task<bool> IsUserLinkedAsync(Guid categoryId, Guid userId)
        => await Context.Journals.AnyAsync(category => category.Id == categoryId && category.User.Id == userId);
}
