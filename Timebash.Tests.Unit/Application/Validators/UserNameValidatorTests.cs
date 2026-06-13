using Bogus;
using FluentValidation.TestHelper;
using Timebash.Application.Validators;

namespace Timebash.Tests.Unit.Application.Validators;

public class UserNameValidatorTests
{
    private static readonly Faker _faker = new();
    private readonly UserNameValidator _validator = new();

    [Fact]
    public void Validate_WithValidName_ShouldPass()
    {
        var result = _validator.TestValidate(_faker.Internet.UserName());
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_EmptyName_ShouldHaveRequiredError()
    {
        var result = _validator.TestValidate(string.Empty);
        result.ShouldHaveValidationErrors().WithErrorCode("REQUIRED");
    }

    [Fact]
    public void Validate_InvalidName_ShouldHaveInvalidFormatError()
    {
        var result = _validator.TestValidate($"{_faker.Internet.UserName()}@");
        result.ShouldHaveValidationErrors().WithErrorCode("INVALID_FORMAT");
    }
}
