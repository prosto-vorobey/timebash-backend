using Microsoft.EntityFrameworkCore;

namespace Timebash.Infrastructure.Repositories;

public class PostgresUserRepository(TimebashDbContext context) : PostgresRepositoryBase<User>(context), IUserRepository
{
    public async Task<User?> GetByEmailAsync(string email)
        => await Context.Users.FirstOrDefaultAsync(user => user.Email == email);

    public async Task<User?> GetByNameAsync(string name)
        => await Context.Users.FirstOrDefaultAsync(user => user.Name == name);

    public async Task<bool> ExistsByEmailAsync(string email)
        => await Context.Users.AnyAsync(user => user.Email == email);

    public async Task<bool> ExistsByNameAsync(string name)
        => await Context.Users.AnyAsync(user => user.Name == name);
}
