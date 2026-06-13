using FluentValidation;

namespace Timebash.Application.Validators;

public class UserEmailValidator : AbstractValidator<string>
{
    public UserEmailValidator()
    {
        RuleFor(email => email)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("Email is required")
            .WithErrorCode("REQUIRED")
            .Matches(@"^[^@\s]+@[^@\s]+\.[^@\s]+$")
            .WithMessage("Invalid email format")
            .WithErrorCode("INVALID_FORMAT");
    }
}
