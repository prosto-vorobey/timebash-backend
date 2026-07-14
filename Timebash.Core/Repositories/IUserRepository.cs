namespace Timebash.Core.Repositories;

public interface IUserRepository : IRepository<User>
{
  Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken);
  Task<User?> GetByNameAsync(string name, CancellationToken cancellationToken);
  Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken);
  Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken);
}
