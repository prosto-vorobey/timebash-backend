namespace Timebash.Core.Exceptions;

public abstract class DomainExceptionBase : Exception
{
    public DomainExceptionBase() { }
    public DomainExceptionBase(string message) : base(message) { }

    public abstract int StatusCode { get; }
    public abstract string Title { get; }
}
