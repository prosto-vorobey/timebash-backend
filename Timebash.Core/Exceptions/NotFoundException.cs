namespace Timebash.Core.Exceptions;

public class NotFoundException : DomainExceptionBase
{
    public NotFoundException() : base() { }
    public NotFoundException(string message) : base(message) {}

    public override int StatusCode => 404;

    public override string Title => "Not found";
}
