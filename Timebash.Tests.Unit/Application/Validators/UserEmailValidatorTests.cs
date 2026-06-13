using Bogus;
using FluentValidation.TestHelper;
using Timebash.Application.Validators;

namespace Timebash.Tests.Unit.Application.Validators;

public class UserEmailValidatorTests
{
    private static readonly Faker _faker = new();
    private readonly UserEmailValidator _validator = new();

    [Fact]
    public void Validate_WithValidEmail_ShouldPass()
    {
        var result = _validator.TestValidate(_faker.Internet.Email());
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_EmptyEmail_ShouldHaveRequiredError()
    {
        var result = _validator.TestValidate(string.Empty);
        result.ShouldHaveValidationErrors().WithErrorCode("REQUIRED");
    }

    [Theory]
    [InlineData("invalid-emal")]
    [InlineData("test@")]
    [InlineData("test@test")]
    [InlineData("@test.test")]
    [InlineData("invalid test@test.test")]
    public void Validate_InvalidEmail_ShouldHaveInvalidFormatError(string email)
    {
        var result = _validator.TestValidate(email);
        result.ShouldHaveValidationErrors().WithErrorCode("INVALID_FORMAT");
    }
}
