using System;
using SchemaValidation.Tests.Base;
using SchemaValidation.Library.Validators;
using SchemaValidation.Core;
using SchemaValidation.Library.Models;
using Xunit;

namespace SchemaValidation.Tests.Validators;

public class StringValidatorTests : ValidationTestBase
{
    private readonly Validator<object> _validator;
    private readonly StringValidator _underlyingValidator;

    public StringValidatorTests()
    {
        _validator = Schema.String();
        _underlyingValidator = ((ValidatorWrapper<string, object, StringValidator>)_validator).UnderlyingValidator;
    }

    [Theory]
    [InlineData("test")]
    [InlineData("")]
    [InlineData(" ")]
    public void Validate_WhenValueIsString_ReturnsTrue(string value)
    {
        // Act
        var result = _validator.Validate(value);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Theory]
    [InlineData(123)]
    [InlineData(true)]
    [InlineData(null)]
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
    [InlineData("", 1, 10)]
    [InlineData("toolongstring", 1, 5)]
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
    public void Pattern_WithValidInput_ShouldPass()
    {
        // Arrange
        var validator = Schema.String().WithMessage("Pattern validation failed");
        ((ValidatorWrapper<string, object, StringValidator>)validator).UnderlyingValidator.Pattern(@"^\d+$");

        // Act
        var result = validator.Validate("123");

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Pattern_WithInvalidInput_ShouldFail()
    {
        // Arrange
        var validator = Schema.String().WithMessage("Pattern validation failed");
        ((ValidatorWrapper<string, object, StringValidator>)validator).UnderlyingValidator.Pattern(@"^\d+$");

        // Act
        var result = validator.Validate("abc");

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains("Pattern validation", result.Errors[0].Message);
    }

    [Fact]
    public void MinLength_WithValidInput_ShouldPass()
    {
        // Arrange
        var validator = Schema.String().WithMessage("Minimum length validation failed");
        ((ValidatorWrapper<string, object, StringValidator>)validator).UnderlyingValidator.MinLength(3);

        // Act
        var result = validator.Validate("abc");

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void MinLength_WithInvalidInput_ShouldFail()
    {
        // Arrange
        var validator = Schema.String().WithMessage("Minimum length validation failed");
        ((ValidatorWrapper<string, object, StringValidator>)validator).UnderlyingValidator.MinLength(3);

        // Act
        var result = validator.Validate("ab");

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains("Minimum length", result.Errors[0].Message);
    }

    [Fact]
    public void MaxLength_WithValidInput_ShouldPass()
    {
        // Arrange
        var validator = Schema.String().WithMessage("Maximum length validation failed");
        ((ValidatorWrapper<string, object, StringValidator>)validator).UnderlyingValidator.MaxLength(3);

        // Act
        var result = validator.Validate("abc");

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void MaxLength_WithInvalidInput_ShouldFail()
    {
        // Arrange
        var validator = Schema.String().WithMessage("Maximum length validation failed");
        ((ValidatorWrapper<string, object, StringValidator>)validator).UnderlyingValidator.MaxLength(3);

        // Act
        var result = validator.Validate("abcd");

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains("Maximum length", result.Errors[0].Message);
    }

    [Fact]
    public void Email_WithInvalidFormat_ShouldFail()
    {
        // Arrange
        var schema = new Dictionary<string, Validator<object>>
        {
            { nameof(User.Email), Schema.String().WithMessage("Invalid email format") }
        };
        var validator = Schema.Object<User>(schema);

        // Act
        var result = validator.Validate(CreateInvalidUser());

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(User.Email));
    }

    [Fact]
    public void PhoneNumber_WithInvalidFormat_ShouldFail()
    {
        // Arrange
        var schema = new Dictionary<string, Validator<object>>
        {
            { nameof(User.PhoneNumber), Schema.String().WithMessage("Invalid phone number format") }
        };
        var validator = Schema.Object<User>(schema);

        // Act
        var result = validator.Validate(CreateInvalidUser());

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(User.PhoneNumber));
    }
} 