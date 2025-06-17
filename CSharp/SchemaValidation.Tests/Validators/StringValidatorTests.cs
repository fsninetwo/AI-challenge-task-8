using System;
using SchemaValidation.Tests.Base;
using SchemaValidation.Validators;
using SchemaValidation.Core;
using Xunit;
using SchemaValidation.Library.Validators;

namespace SchemaValidation.Tests.Validators;

public class StringValidatorTests : ValidationTestBase
{
    private readonly StringValidator _validator;

    public StringValidatorTests()
    {
        _validator = new StringValidator();
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
        _validator.MinLength(minLength);
        _validator.MaxLength(maxLength);

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
        _validator.MinLength(minLength);
        _validator.MaxLength(maxLength);

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
        _validator.Pattern(pattern);

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
        _validator.Pattern(pattern);

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
        var validator = new ValidatorWrapper<string, object, StringValidator>(
            new StringValidator().Pattern(@"^\d+$"));

        // Act
        var result = validator.Validate("123");

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Pattern_WithInvalidInput_ShouldFail()
    {
        // Arrange
        var validator = new ValidatorWrapper<string, object, StringValidator>(
            new StringValidator().Pattern(@"^\d+$"));

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
        var validator = new ValidatorWrapper<string, object, StringValidator>(
            new StringValidator().MinLength(3));

        // Act
        var result = validator.Validate("abc");

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void MinLength_WithInvalidInput_ShouldFail()
    {
        // Arrange
        var validator = new ValidatorWrapper<string, object, StringValidator>(
            new StringValidator().MinLength(3));

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
        var validator = new ValidatorWrapper<string, object, StringValidator>(
            new StringValidator().MaxLength(3));

        // Act
        var result = validator.Validate("abc");

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void MaxLength_WithInvalidInput_ShouldFail()
    {
        // Arrange
        var validator = new ValidatorWrapper<string, object, StringValidator>(
            new StringValidator().MaxLength(3));

        // Act
        var result = validator.Validate("abcd");

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains("Maximum length", result.Errors[0].Message);
    }

    [Fact]
    public void CustomMessage_WithInvalidPattern_ShouldUseCustomMessage()
    {
        // Arrange
        var validator = new ValidatorWrapper<string, object, StringValidator>(
            new StringValidator()
                .Pattern(@"^[A-Z]")
                .WithMessage("Custom pattern message"));

        // Act
        var result = validator.Validate("abc");

        // Assert
        Assert.False(result.IsValid);
        Assert.Equal("Custom pattern message", result.Errors[0].Message);
    }

    [Fact]
    public void CustomMessage_WithInvalidLength_ShouldUseCustomMessage()
    {
        // Arrange
        var validator = new ValidatorWrapper<string, object, StringValidator>(
            new StringValidator()
                .MinLength(5)
                .WithMessage("Custom min length message"));

        // Act
        var result = validator.Validate("abc");

        // Assert
        Assert.False(result.IsValid);
        Assert.Equal("Custom min length message", result.Errors[0].Message);
    }

    [Fact]
    public void Email_WithInvalidFormat_ShouldFail()
    {
        // Arrange
        var user = CreateValidUser() with { Email = "invalid-email" };

        // Act
        var result = UserSchema.Validate(user);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(user.Email));
    }

    [Fact]
    public void PhoneNumber_WithInvalidFormat_ShouldFail()
    {
        // Arrange
        var user = CreateValidUser() with { PhoneNumber = "invalid-phone" };

        // Act
        var result = UserSchema.Validate(user);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(user.PhoneNumber));
    }
} 