using Microsoft.EntityFrameworkCore;

namespace Timebash.Infrastructure.Repositories;

public class PostgresUserRepository(TimebashDbContext context) : PostgresRepositoryBase<User>(context), IUserRepository
{
    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken)
        => await Context.Users.FirstOrDefaultAsync(user => user.Email == email, cancellationToken);

    public async Task<User?> GetByNameAsync(string name, CancellationToken cancellationToken)
        => await Context.Users.FirstOrDefaultAsync(user => user.Name == name, cancellationToken);

    public async Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken)
        => await Context.Users.AnyAsync(user => user.Email == email, cancellationToken);

    public async Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken)
        => await Context.Users.AnyAsync(user => user.Name == name, cancellationToken);
}
