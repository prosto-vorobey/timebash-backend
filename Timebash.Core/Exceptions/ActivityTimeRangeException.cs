namespace Timebash.Core.Exceptions;

public class ActivityTimeRangeException : BadRequestException
{
    public ActivityTimeRangeException() { }
    public ActivityTimeRangeException(string message) : base(message) { }
}