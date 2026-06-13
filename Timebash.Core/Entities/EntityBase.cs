using Timebash.Core.Utilities;

namespace Timebash.Core.Entities;

public abstract class EntityBase(Guid id) : IEntity
{
    public Guid Id { get; init; } = Ensure.NotEmpty(id, nameof(id));
}
