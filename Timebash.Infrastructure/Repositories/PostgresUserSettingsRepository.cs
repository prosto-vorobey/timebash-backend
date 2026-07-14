using Microsoft.EntityFrameworkCore;

namespace Timebash.Infrastructure.Repositories;

public class PostgresUserSettingsRepository(TimebashDbContext context) : IUserSettingsRepository
{
    private readonly TimebashDbContext _context = context;

    public async Task<UserSettings?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        => await _context.UserSettings.SingleOrDefaultAsync(settings => settings.UserId == id, cancellationToken);

    public void Add(UserSettings userSettings) => _context.UserSettings.Add(userSettings);

    public void Update(UserSettings userSettings) => _context.UserSettings.Update(userSettings);

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var settings = await GetByIdAsync(id, cancellationToken);
        if (settings is not null) _context.UserSettings.Remove(settings);
    }
}
