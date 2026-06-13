namespace Timebash.Core.Exceptions;

public class ResourceConflictException : ConflictException
{
    public ResourceConflictException(string field) : base()
    {
        Field = field;
    }

    public ResourceConflictException(string field, string message) : base(message)
    {
        Field = field;
    }

    public string Field;
}
