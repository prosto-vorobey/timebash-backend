namespace Timebash.Core.Contracts;

public interface IUnitOfWork
{
    Task SaveChangesAsync();
}
