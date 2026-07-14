namespace Timebash.Core.Repositories;

public interface IRepository<T> where T : IEntity
{
  Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
  Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken);
  void Add(T entity);
  void Update(T entity);
  void Delete(T entity);
  Task DeleteAsync(Guid id, CancellationToken cancellationToken);
}
