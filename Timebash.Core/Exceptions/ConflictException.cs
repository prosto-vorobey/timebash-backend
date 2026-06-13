namespace Timebash.Core.Exceptions;

public class ConflictException : DomainExceptionBase
{
    public ConflictException() : base() { }
    public ConflictException(string message) : base(message) { }

    public override int StatusCode => 409;

    public override string Title => "Conflict";
}
