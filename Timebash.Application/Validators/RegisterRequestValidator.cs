using FluentValidation;
using Timebash.Core.DTOs.Requests;

namespace Timebash.Application.Validators;

public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator(UserNameValidator userNameValidator, UserEmailValidator userEmailValidator, PasswordValidator passwordValidator)
    {
        RuleFor(request => request.Name)
            .SetValidator(userNameValidator);

        RuleFor(request => request.Email)
            .SetValidator(userEmailValidator);

        RuleFor(request => request.Password)
            .SetValidator(passwordValidator);
    }
}
