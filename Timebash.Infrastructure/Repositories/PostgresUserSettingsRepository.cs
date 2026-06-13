using Microsoft.EntityFrameworkCore;

namespace Timebash.Infrastructure.Repositories;

public class PostgresUserSettingsRepository(TimebashDbContext context) : IUserSettingsRepository
{
    private readonly TimebashDbContext _context = context;

    public async Task<UserSettings?> GetByIdAsync(Guid id)
        => await _context.UserSettings.SingleOrDefaultAsync(settings => settings.UserId == id);

    public void Add(UserSettings userSettings) => _context.UserSettings.Add(userSettings);

    public void Update(UserSettings userSettings) => _context.UserSettings.Update(userSettings);

    public async Task DeleteAsync(Guid id)
    {
        var settings = await GetByIdAsync(id);
        if (settings is not null) _context.UserSettings.Remove(settings);
    }
}
