using System;
using SchemaValidation.Tests.Base;
using SchemaValidation.Library.Validators;
using SchemaValidation.Core;
using SchemaValidation.Library.Models;
using SchemaValidation.Library.Schemas;
using Xunit;

namespace SchemaValidation.Tests.Validators;

public class NumberValidatorTests : ValidationTestBase
{
    private readonly NumberValidator _validator;

    public NumberValidatorTests()
    {
        _validator = new NumberValidator();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(42)]
    [InlineData(3.14)]
    public void Validate_WhenValueIsNumber_ReturnsTrue(double value)
    {
        // Act
        var result = _validator.Validate(value);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Theory]
    [InlineData("not a number")]
    [InlineData(null)]
    public void Validate_WhenValueIsNotNumber_ReturnsFalse(object value)
    {
        // Act
        var result = _validator.Validate((double)value);

        // Assert
        Assert.False(result.IsValid);
        Assert.NotEmpty(result.Errors);
    }

    [Theory]
    [InlineData(0, 10, 5)]
    [InlineData(-10, 0, -5)]
    public void Validate_WhenValueIsInRange_ReturnsTrue(double min, double max, double value)
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
    [InlineData(0, 10, -5)]
    [InlineData(0, 10, 15)]
    [InlineData(-10, 0, 5)]
    public void Validate_WhenValueIsOutOfRange_ReturnsFalse(double min, double max, double value)
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
    public void Validate_WithCustomMessage_UsesCustomMessageOnError()
    {
        // Arrange
        var customMessage = "Value must be between 0 and 10";
        _validator.SetRange(0, 10).WithMessage(customMessage);

        // Act
        var result = _validator.Validate(-5);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.Message == customMessage);
    }

    [Fact]
    public void Min_WithValidInput_ShouldPass()
    {
        // Arrange
        _validator.Min(0);

        // Act
        var result = _validator.Validate(5);

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Min_WithInvalidInput_ShouldFail()
    {
        // Arrange
        _validator.Min(0);

        // Act
        var result = _validator.Validate(-1);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains("Value must be greater than or equal to 0", result.Errors[0].Message);
    }

    [Fact]
    public void Max_WithValidInput_ShouldPass()
    {
        // Arrange
        _validator.Max(10);

        // Act
        var result = _validator.Validate(5);

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Max_WithInvalidInput_ShouldFail()
    {
        // Arrange
        _validator.Max(10);

        // Act
        var result = _validator.Validate(15);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains("Value must be less than or equal to 10", result.Errors[0].Message);
    }

    [Fact]
    public void NonNegative_WithValidInput_ShouldPass()
    {
        // Arrange
        _validator.NonNegative();

        // Act
        var result = _validator.Validate(5);

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void NonNegative_WithInvalidInput_ShouldFail()
    {
        // Arrange
        _validator.NonNegative();

        // Act
        var result = _validator.Validate(-1);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains("Value must be greater than or equal to 0", result.Errors[0].Message);
    }

    [Fact]
    public void SetRange_WithValidInput_ShouldPass()
    {
        // Arrange
        _validator.SetRange(0, 10);

        // Act
        var result = _validator.Validate(5);

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void SetRange_WithInvalidInput_ShouldFail()
    {
        // Arrange
        _validator.SetRange(0, 10);

        // Act
        var result = _validator.Validate(15);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains("Value must be less than or equal to 10", result.Errors[0].Message);
    }

    [Fact]
    public void Age_WithValidInput_ShouldPass()
    {
        // Arrange
        var validator = UserSchema.Create();
        var user = CreateValidUser();

        // Act
        var result = validator.Validate(user);

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Age_WithInvalidInput_ShouldFail()
    {
        // Arrange
        var validator = UserSchema.Create();
        var user = CreateInvalidUser();

        // Act
        var result = validator.Validate(user);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(User.Age));
    }
} 