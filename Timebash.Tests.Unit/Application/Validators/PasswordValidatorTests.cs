using FluentAssertions;
using FluentValidation.TestHelper;
using Timebash.Application.Validators;

namespace Timebash.Tests.Unit.Application.Validators;

public class PasswordValidatorTests
{
    private readonly PasswordValidator _validator = new();

    [Fact]
    public void Validate_WithValidPassword_ShouldPass()
        => _validator.TestValidate("123456")
            .ShouldNotHaveAnyValidationErrors();

    [Fact]
    public void Validate_EmptyPassword_ShouldHaveRequiredError()
        => _validator.TestValidate(string.Empty)
            .ShouldHaveValidationErrors()
            .WithErrorCode("REQUIRED");

    [Fact]
    public void Validate_ShortPassword_ShouldHaveShortError()
        => _validator.TestValidate("12345")
            .ShouldHaveValidationErrors()
            .WithErrorCode("SHORT");

    [Fact]
    public void Validate_EmptyPassword_ShouldHaveTwoErrors()
    {
        var result = _validator.TestValidate("");
        result.Errors.Should().HaveCount(2);
        result.ShouldHaveValidationErrors().WithErrorCode("REQUIRED");
        result.ShouldHaveValidationErrors().WithErrorCode("SHORT");
    }
}
