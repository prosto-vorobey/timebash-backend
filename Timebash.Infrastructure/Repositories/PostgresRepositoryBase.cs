using Microsoft.EntityFrameworkCore;

namespace Timebash.Infrastructure.Repositories;

public abstract class PostgresRepositoryBase<T>(TimebashDbContext context) : IRepository<T> where T : EntityBase
{
    protected TimebashDbContext Context => context;

    public async Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken) => await Context.Set<T>().FindAsync([id], cancellationToken);

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken)
        => await Context.Set<T>().AnyAsync(entity => entity.Id == id, cancellationToken);

    public void Add(T entity) => Context.Set<T>().Add(entity);

    public void Update(T entity) => Context.Set<T>().Update(entity);

    public void Delete(T entity) => Context.Set<T>().Remove(entity);

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var entity = await GetByIdAsync(id, cancellationToken);
        if (entity is not null) Delete(entity);
    }
}
