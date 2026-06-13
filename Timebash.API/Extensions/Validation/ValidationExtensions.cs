using FluentValidation.Results;

namespace Timebash.API.Extensions.Validation;

public static class ValidationExtensions
{
    public static ValidationErrorResponse ToValidationErrorResponse(
        this ValidationResult validationResult,
        string instance,
        string traceId)
    {
        var errors = new Dictionary<string, List<ValidationErrorItem>>();

        foreach (var error in validationResult.Errors)
        {
            var key = error.PropertyName;

            if (!errors.TryGetValue(key, out var value))
            {
                value = [];
                errors[key] = value;
            }

            value.Add(new ValidationErrorItem(
                error.ErrorMessage,
                error.ErrorCode));
        }

        return new ValidationErrorResponse(
            "Validation Failed",
            StatusCodes.Status400BadRequest,
            instance,
            traceId,
            errors.ToDictionary(
                pair => pair.Key,
                pair => (IReadOnlyList<ValidationErrorItem>)pair.Value.AsReadOnly())
        );
    }
}
