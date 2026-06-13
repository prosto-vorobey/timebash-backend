using Bogus;
using FluentValidation.TestHelper;
using Timebash.Application.Validators;
using Timebash.Core.DTOs.Requests;

namespace Timebash.Tests.Unit.Application.Validators;

public class RegisterRequestValidatorTests
{
    private static readonly Faker _faker = new();
    private readonly RegisterRequestValidator _validator = new(new(), new(), new());

    [Fact]
    public void Validate_ValidRequest_ShouldPass()
    {
        var result = _validator.TestValidate(new RegisterRequest(_faker.Internet.UserName(), _faker.Internet.Email(), "123456"));
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_InvalidName_ShouldHaveNameError()
    {
        var result = _validator.TestValidate(new RegisterRequest(string.Empty, _faker.Internet.Email(), "123456"));
        result.ShouldHaveValidationErrorFor(response => response.Name).WithErrorCode("REQUIRED");
        result.ShouldNotHaveValidationErrorFor(response => response.Email);
        result.ShouldNotHaveValidationErrorFor(response => response.Password);
    }

    [Fact]
    public void Validate_InvalidEmail_ShouldHaveEmailError()
    {
        var result = _validator.TestValidate(new RegisterRequest(_faker.Internet.UserName(), string.Empty, "123456"));
        result.ShouldHaveValidationErrorFor(response => response.Email).WithErrorCode("REQUIRED");
        result.ShouldNotHaveValidationErrorFor(response => response.Name);
        result.ShouldNotHaveValidationErrorFor(response => response.Password);
    }

    [Fact]
    public void Validate_InvalidName_ShouldHavePasswordError()
    {
        var result = _validator.TestValidate(new RegisterRequest(_faker.Internet.UserName(), _faker.Internet.Email(), string.Empty));
        result.ShouldHaveValidationErrorFor(response => response.Password).WithErrorCode("REQUIRED");
        result.ShouldNotHaveValidationErrorFor(response => response.Name);
        result.ShouldNotHaveValidationErrorFor(response => response.Email);
    }

    [Fact]
    public void Validate_InvalidRequest_ShouldHaveTwoErrors()
    {
        var result = _validator.TestValidate(new RegisterRequest($"{_faker.Internet.UserName()}@", string.Empty, "123456"));
        result.ShouldHaveValidationErrorFor(response => response.Name).WithErrorCode("INVALID_FORMAT");
        result.ShouldHaveValidationErrorFor(response => response.Email).WithErrorCode("REQUIRED");
        result.ShouldNotHaveValidationErrorFor(response => response.Password);
    }
}
