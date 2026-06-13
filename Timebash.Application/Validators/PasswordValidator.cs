using FluentValidation;

namespace Timebash.Application.Validators;

public class PasswordValidator : AbstractValidator<string>
{
    public PasswordValidator()
    {
        RuleFor(password => password)
            .NotEmpty()
            .WithMessage("Password is required")
            .WithErrorCode("REQUIRED")
            .MinimumLength(6)
            .WithMessage("Password must be at least 6 characters.")
            .WithErrorCode("SHORT");
    }
}
