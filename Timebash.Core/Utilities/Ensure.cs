using System.Text.RegularExpressions;
using Timebash.Core.Exceptions;

namespace Timebash.Core.Utilities;

public static class Ensure
{
    public static Guid NotEmpty(Guid value, string paramName)
    {
        if (value == Guid.Empty)
            throw new DomainValidationException($"{paramName} cannot be empty.");
        return value;
    }

    public static string NotNullOrWhiteSpace(string value, string paramName)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainValidationException($"{paramName} cannot be null, empty or whitespace");
        return value;
    }

    public static string ValidHexColor(string value, string paramName)
    {
        NotNullOrWhiteSpace(value, paramName);
        if (!Regex.IsMatch(value, "^#[0-9A-Fa-f]{6}$|^#[0-9A-Fa-f]{3}$", RegexOptions.IgnoreCase))
            throw new DomainValidationException($"{paramName} must be a valid hex color.");
        
        return value;
    }
}