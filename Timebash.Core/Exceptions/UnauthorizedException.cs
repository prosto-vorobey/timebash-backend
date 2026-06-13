namespace Timebash.Core.Exceptions;

public class UnauthorizedException : DomainExceptionBase
{
    public UnauthorizedException() : base() { }
    public UnauthorizedException(string message) : base(message) { }

    public override int StatusCode => 401;

    public override string Title => "Unauthorized";
}
