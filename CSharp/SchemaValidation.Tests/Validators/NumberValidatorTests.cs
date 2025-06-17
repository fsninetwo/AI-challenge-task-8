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
    private readonly Validator<object> _validator;
    private readonly NumberValidator _underlyingValidator;

    public NumberValidatorTests()
    {
        _validator = Schema.Number();
        _underlyingValidator = ((SchemaValidation.Core.ValidatorWrapper<double, object, NumberValidator>)_validator).UnderlyingValidator;
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
    [InlineData(null)]
    [InlineData("not a number")]
    [InlineData(true)]
    public void Validate_WhenValueIsNotNumber_ReturnsFalse(object value)
    {
        // Act
        var result = _validator.Validate(value);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains("Value must be a number", result.Errors[0].Message);
    }

    [Theory]
    [InlineData(0, 10, 5)]
    [InlineData(-10, 0, -5)]
    public void Validate_WhenValueIsInRange_ReturnsTrue(double min, double max, double value)
    {
        // Arrange
        _underlyingValidator.SetRange(min, max);

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
        _underlyingValidator.SetRange(min, max);

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
        _underlyingValidator.SetRange(0, 10);
        _validator.WithMessage(customMessage);

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
        _underlyingValidator.SetMin(0);

        // Act
        var result = _validator.Validate(5);

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Min_WithInvalidInput_ShouldFail()
    {
        // Arrange
        _underlyingValidator.SetMin(0);

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
        _underlyingValidator.SetMax(10);

        // Act
        var result = _validator.Validate(5);

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Max_WithInvalidInput_ShouldFail()
    {
        // Arrange
        _underlyingValidator.SetMax(10);

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
        _underlyingValidator.SetNonNegative();

        // Act
        var result = _validator.Validate(5);

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void NonNegative_WithInvalidInput_ShouldFail()
    {
        // Arrange
        _underlyingValidator.SetNonNegative();

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
        _underlyingValidator.SetRange(0, 10);

        // Act
        var result = _validator.Validate(5);

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void SetRange_WithInvalidInput_ShouldFail()
    {
        // Arrange
        _underlyingValidator.SetRange(0, 10);

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
        var schema = new Dictionary<string, Validator<object>>
        {
            { nameof(User.Age), Schema.Number().WithMessage("Age must be non-negative") }
        };
        var validator = Schema.ObjectAsValidator<User>(schema);
        ((ValidatorWrapper<double, object, NumberValidator>)schema[nameof(User.Age)]).UnderlyingValidator.SetNonNegative();

        // Act
        var result = validator.Validate(new User
        {
            Id = "123",
            Name = "John",
            Email = "john@example.com",
            Age = 25,
            IsActive = true
        });

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Age_WithInvalidInput_ShouldFail()
    {
        // Arrange
        var schema = new Dictionary<string, Validator<object>>
        {
            { nameof(User.Age), Schema.Number().WithMessage("Age must be non-negative") }
        };
        var validator = Schema.ObjectAsValidator<User>(schema);
        ((ValidatorWrapper<double, object, NumberValidator>)schema[nameof(User.Age)]).UnderlyingValidator.SetNonNegative();

        // Act
        var result = validator.Validate(new User
        {
            Id = "123",
            Name = "John",
            Email = "john@example.com",
            Age = -1,
            IsActive = true
        });

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(User.Age));
    }
} 