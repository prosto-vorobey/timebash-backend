using Bogus;
using FluentAssertions;
using Timebash.Core.Exceptions;
using Timebash.Core.Utilities;
using Timebash.Tests.Unit.Core.TestData;

namespace Timebash.Tests.Unit.Core.Utilities;

public class EnsureTests
{
    private static readonly Faker _faker = new();

    [Fact]
    public void NotEmpty_WithValidGuid_ShouldReturnValue()
    {
        var id = Guid.NewGuid();
        var result = Ensure.NotEmpty(id, string.Empty);

        result.Should().Be(id);
    }

    [Fact]
    public void NotEmpty_EmptyGuid_ShouldThrowDomainValidationException()
        => FluentActions
            .Invoking(() => Ensure.NotEmpty(Guid.Empty, string.Empty))
            .Should()
            .Throw<DomainValidationException>();

    [Fact]
    public void NotNullOrWhiteSpace_WithValidValue_ShouldReturnValue()
    {
        var value = _faker.Lorem.Word();
        var result = Ensure.NotNullOrWhiteSpace(value, string.Empty);

        result.Should().Be(value);
    }

    [Theory]
    [ClassData(typeof(NullOrWhitespaceStringData))]
    public void NotNullOrWhiteSpace_EmptyValue_ShouldThrowDomainValidationException(string value)
        => FluentActions
            .Invoking(() => Ensure.NotNullOrWhiteSpace(value, string.Empty))
            .Should()
            .Throw<DomainValidationException>();

    [Theory]
    [ClassData(typeof(ValidHexColorData))]
    public void ValidHexColor_WithValidColor_ShouldReturnValue(string color)
    {
        var result = Ensure.ValidHexColor(color, string.Empty);

        result.Should().Be(color);
    }

    [Theory]
    [ClassData(typeof(NullOrWhitespaceStringData))]
    [ClassData(typeof(InvalidHexColorData))]
    public void ValidHexColor_InvalidColor_ShouldThrowDomainValidationException(string color)
        => FluentActions
            .Invoking(() => Ensure.ValidHexColor(color, string.Empty))
            .Should()
            .Throw<DomainValidationException>();
}
