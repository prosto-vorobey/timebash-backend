namespace Timebash.Core.Repositories;
public interface IUserSettingsRepository
{
    public Task<UserSettings?> GetByIdAsync(Guid id);
    public void Add(UserSettings userSettings);
    public void Update(UserSettings userSettings);
    public Task DeleteAsync(Guid id);
}
