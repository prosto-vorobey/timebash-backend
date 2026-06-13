namespace Timebash.Core.Repositories;

public interface IUserRepository : IRepository<User>
{
  Task<User?> GetByEmailAsync(string email);
  Task<User?> GetByNameAsync(string name);
  Task<bool> ExistsByEmailAsync(string email);
  Task<bool> ExistsByNameAsync(string name);
}
