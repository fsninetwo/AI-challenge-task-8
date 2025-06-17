using System;
using SchemaValidation.Tests.Base;
using SchemaValidation.Library.Validators;
using SchemaValidation.Core;
using Xunit;

namespace SchemaValidation.Tests.Validators;

public class StringValidatorTests : ValidationTestBase
{
    private readonly Validator<object> _validator;
    private readonly StringValidator _underlyingValidator;

    public StringValidatorTests()
    {
        _validator = Schema.String();
        _underlyingValidator = ((SchemaValidation.Core.ValidatorWrapper<string, object, StringValidator>)_validator).UnderlyingValidator;
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("test")]
    public void Validate_WhenValueIsString_ReturnsTrue(string value)
    {
        // Act
        var result = _validator.Validate(value);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Theory]
    [InlineData(null)]
    [InlineData(123)]
    [InlineData(true)]
    public void Validate_WhenValueIsNotString_ReturnsFalse(object value)
    {
        // Act
        var result = _validator.Validate(value);

        // Assert
        Assert.False(result.IsValid);
        Assert.NotEmpty(result.Errors);
    }

    [Theory]
    [InlineData("test", 1, 10)]
    [InlineData("a", 1, 1)]
    public void Validate_WhenLengthIsInRange_ReturnsTrue(string value, int minLength, int maxLength)
    {
        // Arrange
        _underlyingValidator.MinLength(minLength);
        _underlyingValidator.MaxLength(maxLength);

        // Act
        var result = _validator.Validate(value);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Theory]
    [InlineData("toolongstring", 1, 5)]
    [InlineData("", 1, 10)]
    public void Validate_WhenLengthIsOutOfRange_ReturnsFalse(string value, int minLength, int maxLength)
    {
        // Arrange
        _underlyingValidator.MinLength(minLength);
        _underlyingValidator.MaxLength(maxLength);

        // Act
        var result = _validator.Validate(value);

        // Assert
        Assert.False(result.IsValid);
        Assert.NotEmpty(result.Errors);
    }

    [Theory]
    [InlineData("test@example.com", @"^[^@\s]+@[^@\s]+\.[^@\s]+$")]
    [InlineData("123-456-7890", @"^\d{3}-\d{3}-\d{4}$")]
    public void Validate_WhenMatchesPattern_ReturnsTrue(string value, string pattern)
    {
        // Arrange
        _underlyingValidator.Pattern(pattern);

        // Act
        var result = _validator.Validate(value);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Theory]
    [InlineData("invalid-email", @"^[^@\s]+@[^@\s]+\.[^@\s]+$")]
    [InlineData("123456789", @"^\d{3}-\d{3}-\d{4}$")]
    public void Validate_WhenDoesNotMatchPattern_ReturnsFalse(string value, string pattern)
    {
        // Arrange
        _underlyingValidator.Pattern(pattern);

        // Act
        var result = _validator.Validate(value);

        // Assert
        Assert.False(result.IsValid);
        Assert.NotEmpty(result.Errors);
    }

    [Fact]
    public void MinLength_WithValidInput_ShouldPass()
    {
        // Arrange
        _underlyingValidator.MinLength(3);

        // Act
        var result = _validator.Validate("test");

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void MinLength_WithInvalidInput_ShouldFail()
    {
        // Arrange
        _underlyingValidator.MinLength(3);

        // Act
        var result = _validator.Validate("ab");

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains("Minimum length", result.Errors[0].Message);
    }

    [Fact]
    public void MaxLength_WithValidInput_ShouldPass()
    {
        // Arrange
        _underlyingValidator.MaxLength(5);

        // Act
        var result = _validator.Validate("test");

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void MaxLength_WithInvalidInput_ShouldFail()
    {
        // Arrange
        _underlyingValidator.MaxLength(3);

        // Act
        var result = _validator.Validate("test");

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains("Maximum length", result.Errors[0].Message);
    }

    [Fact]
    public void Pattern_WithValidInput_ShouldPass()
    {
        // Arrange
        _underlyingValidator.Pattern(@"^[a-z]+$");

        // Act
        var result = _validator.Validate("test");

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Pattern_WithInvalidInput_ShouldFail()
    {
        // Arrange
        _underlyingValidator.Pattern(@"^[a-z]+$");

        // Act
        var result = _validator.Validate("Test123");

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains("Pattern", result.Errors[0].Message);
    }

    [Fact]
    public void Email_WithInvalidFormat_ShouldFail()
    {
        // Arrange
        _underlyingValidator.Email();

        // Act
        var result = _validator.Validate("invalid-email");

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains("Email", result.Errors[0].Message);
    }

    [Fact]
    public void PhoneNumber_WithInvalidFormat_ShouldFail()
    {
        // Arrange
        _underlyingValidator.PhoneNumber();

        // Act
        var result = _validator.Validate("123456789");

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains("Phone number", result.Errors[0].Message);
    }
} 