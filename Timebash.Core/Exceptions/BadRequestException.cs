namespace Timebash.Core.Exceptions;

public class BadRequestException : DomainExceptionBase
{
    public BadRequestException() { }
    public BadRequestException(string message) : base(message) { }

    public override int StatusCode => 400;
    public override string Title => "Bad request";
}
