using FluentValidation;

namespace Timebash.Application.Validators;

public class UserNameValidator : AbstractValidator<string>
{
    public UserNameValidator()
    {
        RuleFor(name => name)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("Name is required")
            .WithErrorCode("REQUIRED")
            .Must(name => !name.Contains('@'))
            .WithMessage("{PropertyName} cannot contain '@' symbol.")
            .WithErrorCode("INVALID_FORMAT");
    }
}
