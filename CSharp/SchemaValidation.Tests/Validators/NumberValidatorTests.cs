using System;
using SchemaValidation.Tests.Base;
using SchemaValidation.Validators;
using SchemaValidation.Core;
using SchemaValidation.Models;
using Xunit;
using SchemaValidation.Library.Validators;

namespace SchemaValidation.Tests.Validators;

public class NumberValidatorTests : ValidationTestBase
{
    private readonly NumberValidator _validator;

    public NumberValidatorTests()
    {
        _validator = new NumberValidator();
    }

    [Theory]
    [InlineData(42)]
    [InlineData(3.14)]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_WhenValueIsNumber_ReturnsTrue(object value)
    {
        // Act
        var result = _validator.Validate(value);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Theory]
    [InlineData("123")]
    [InlineData(true)]
    [InlineData(null)]
    public void Validate_WhenValueIsNotNumber_ReturnsFalse(object value)
    {
        // Act
        var result = _validator.Validate(value);

        // Assert
        Assert.False(result.IsValid);
        Assert.NotEmpty(result.Errors);
    }

    [Theory]
    [InlineData(5, 1, 10)]
    [InlineData(1, 1, 1)]
    public void Validate_WhenValueIsInRange_ReturnsTrue(double value, double min, double max)
    {
        // Arrange
        _validator.SetRange(min, max);

        // Act
        var result = _validator.Validate(value);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Theory]
    [InlineData(0, 1, 10)]
    [InlineData(11, 1, 10)]
    public void Validate_WhenValueIsOutOfRange_ReturnsFalse(double value, double min, double max)
    {
        // Arrange
        _validator.SetRange(min, max);

        // Act
        var result = _validator.Validate(value);

        // Assert
        Assert.False(result.IsValid);
        Assert.NotEmpty(result.Errors);
    }

    [Fact]
    public void Min_WithValidInput_ShouldPass()
    {
        // Arrange
        _validator.Min(0);
        var validator = new ValidatorWrapper<double, double, NumberValidator>(_validator);

        // Act
        var result = validator.Validate(10);

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Min_WithInvalidInput_ShouldFail()
    {
        // Arrange
        _validator.Min(0);
        var validator = new ValidatorWrapper<double, double, NumberValidator>(_validator);

        // Act
        var result = validator.Validate(-1);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains("must be greater than or equal to 0", result.Errors[0].Message);
    }

    [Fact]
    public void Max_WithValidInput_ShouldPass()
    {
        // Arrange
        _validator.Max(100);
        var validator = new ValidatorWrapper<double, double, NumberValidator>(_validator);

        // Act
        var result = validator.Validate(50);

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Max_WithInvalidInput_ShouldFail()
    {
        // Arrange
        _validator.Max(100);
        var validator = new ValidatorWrapper<double, double, NumberValidator>(_validator);

        // Act
        var result = validator.Validate(101);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains("must be less than or equal to 100", result.Errors[0].Message);
    }

    [Fact]
    public void Integer_WithValidInput_ShouldPass()
    {
        // Arrange
        _validator.Integer();
        var validator = new ValidatorWrapper<double, double, NumberValidator>(_validator);

        // Act
        var result = validator.Validate(42);

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Integer_WithInvalidInput_ShouldFail()
    {
        // Arrange
        _validator.Integer();
        var validator = new ValidatorWrapper<double, double, NumberValidator>(_validator);

        // Act
        var result = validator.Validate(42.5);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains("must be an integer", result.Errors[0].Message);
    }

    [Fact]
    public void NonNegative_WithValidInput_ShouldPass()
    {
        // Arrange
        _validator.NonNegative();
        var validator = new ValidatorWrapper<double, double, NumberValidator>(_validator);

        // Act
        var result = validator.Validate(0);

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void NonNegative_WithInvalidInput_ShouldFail()
    {
        // Arrange
        _validator.NonNegative();
        var validator = new ValidatorWrapper<double, double, NumberValidator>(_validator);

        // Act
        var result = validator.Validate(-1);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains("must be non-negative", result.Errors[0].Message);
    }

    [Fact]
    public void CustomMessage_WithInvalidInput_ShouldUseCustomMessage()
    {
        // Arrange
        _validator.Min(0);
        _validator.WithMessage("Custom min message");
        var validator = new ValidatorWrapper<double, double, NumberValidator>(_validator);

        // Act
        var result = validator.Validate(-1);

        // Assert
        Assert.False(result.IsValid);
        Assert.Equal("Custom min message", result.Errors[0].Message);
    }

    [Fact]
    public void CombinedValidation_ShouldWork()
    {
        // Arrange
        _validator.Min(0);
        _validator.Max(100);
        _validator.Integer();
        var validator = new ValidatorWrapper<double, double, NumberValidator>(_validator);

        // Act & Assert
        var validResult = validator.Validate(50);
        Assert.True(validResult.IsValid);

        var tooSmallResult = validator.Validate(-1);
        Assert.False(tooSmallResult.IsValid);
        Assert.Contains("must be greater than or equal to 0", tooSmallResult.Errors[0].Message);

        var tooBigResult = validator.Validate(101);
        Assert.False(tooBigResult.IsValid);
        Assert.Contains("must be less than or equal to 100", tooBigResult.Errors[0].Message);

        var notIntegerResult = validator.Validate(50.5);
        Assert.False(notIntegerResult.IsValid);
        Assert.Contains("must be an integer", notIntegerResult.Errors[0].Message);
    }

    [Fact]
    public void Age_WithInvalidInput_ShouldFail()
    {
        // Arrange
        var user = CreateValidUser() with { Age = -1 };

        // Act
        var result = UserSchema.Validate(user);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(User.Age));
    }
} 