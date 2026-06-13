namespace Timebash.Core.Exceptions;

public class DomainValidationException : BadRequestException
{
    public DomainValidationException() { }
    public DomainValidationException(string message) : base(message) { } 
}
