using System;
using System.Collections.Generic;
using SchemaValidation.Tests.Base;
using SchemaValidation.Library.Validators;
using SchemaValidation.Core;
using SchemaValidation.Library.Models;
using SchemaValidation.Library.Schemas;
using Xunit;

namespace SchemaValidation.Tests.Validators;

public class ValidatorWrapperTests : ValidationTestBase
{
    [Fact]
    public void StringValidator_WithValidInput_ShouldPass()
    {
        // Arrange
        var validator = Schema.String().WithMessage("String validation failed");
        ((ValidatorWrapper<string, object, StringValidator>)validator).UnderlyingValidator.MinLength(3);

        // Act
        var result = validator.Validate("test");

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void StringValidator_WithInvalidInput_ShouldFail()
    {
        // Arrange
        var validator = Schema.String().WithMessage("String validation failed");
        ((ValidatorWrapper<string, object, StringValidator>)validator).UnderlyingValidator.MinLength(3);

        // Act
        var result = validator.Validate("ab");

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains("Minimum length", result.Errors[0].Message);
    }

    [Fact]
    public void NumberValidator_WithValidInput_ShouldPass()
    {
        // Arrange
        var validator = Schema.Number().WithMessage("Number validation failed");
        ((ValidatorWrapper<double, object, NumberValidator>)validator).UnderlyingValidator.NonNegative();

        // Act
        var result = validator.Validate(10.0);

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void NumberValidator_WithInvalidInput_ShouldFail()
    {
        // Arrange
        var validator = Schema.Number().WithMessage("Number validation failed");
        ((ValidatorWrapper<double, object, NumberValidator>)validator).UnderlyingValidator.NonNegative();

        // Act
        var result = validator.Validate(-1.0);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains("must be non-negative", result.Errors[0].Message);
    }

    [Fact]
    public void NumberValidator_WithIntegerInput_ShouldConvertAndPass()
    {
        // Arrange
        var validator = Schema.Number().WithMessage("Number validation failed");
        ((ValidatorWrapper<double, object, NumberValidator>)validator).UnderlyingValidator.NonNegative();

        // Act
        var result = validator.Validate(42.0); // Integer input

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void NumberValidator_WithDecimalInput_ShouldConvertAndPass()
    {
        // Arrange
        var validator = Schema.Number().WithMessage("Number validation failed");
        ((ValidatorWrapper<double, object, NumberValidator>)validator).UnderlyingValidator.NonNegative();

        // Act
        var result = validator.Validate(42.5); // Decimal input

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void ArrayValidator_WithValidInput_ShouldPass()
    {
        // Arrange
        var validator = Schema.Array(Schema.String()).WithMessage("Array validation failed");
        ((ValidatorWrapper<IEnumerable<string>, object, ArrayValidator<string>>)validator).UnderlyingValidator.MinLength(2);

        // Act
        var result = validator.Validate(new[] { "item1", "item2" });

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void ArrayValidator_WithInvalidInput_ShouldFail()
    {
        // Arrange
        var validator = Schema.Array(Schema.String()).WithMessage("Array validation failed");
        ((ValidatorWrapper<IEnumerable<string>, object, ArrayValidator<string>>)validator).UnderlyingValidator.MinLength(2);

        // Act
        var result = validator.Validate(new[] { "item1" });

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains("Array must have at least 2 items", result.Errors[0].Message);
    }

    [Fact]
    public void ArrayValidator_WithListInput_ShouldPass()
    {
        // Arrange
        var validator = Schema.Array(Schema.String()).WithMessage("Array validation failed");
        ((ValidatorWrapper<IEnumerable<string>, object, ArrayValidator<string>>)validator).UnderlyingValidator.MinLength(2);

        // Act
        var result = validator.Validate(new List<string> { "item1", "item2" }); // List<T> input

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void ArrayValidator_WithHashSetInput_ShouldPass()
    {
        // Arrange
        var validator = Schema.Array(Schema.String()).WithMessage("Array validation failed");
        ((ValidatorWrapper<IEnumerable<string>, object, ArrayValidator<string>>)validator).UnderlyingValidator.MinLength(2);

        // Act
        var result = validator.Validate(new HashSet<string> { "item1", "item2" }); // HashSet<T> input

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void ObjectValidator_WithValidInput_ShouldPass()
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
    public void ObjectValidator_WithInvalidInput_ShouldFail()
    {
        // Arrange
        var validator = UserSchema.Create();
        var user = CreateInvalidUser();

        // Act
        var result = validator.Validate(user);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(User.Name));
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(User.Email));
    }

    [Fact]
    public void Optional_WithNullInput_ShouldPass()
    {
        // Arrange
        var validator = Schema.String().WithMessage("String validation failed");
        ((ValidatorWrapper<string, object, StringValidator>)validator).UnderlyingValidator.MinLength(3);

        // Act
        var result = validator.Validate(null);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains("Expected value of type String", result.Errors[0].Message);
    }

    [Fact]
    public void Optional_WithInvalidInput_ShouldFail()
    {
        // Arrange
        var validator = Schema.String().WithMessage("String validation failed");
        ((ValidatorWrapper<string, object, StringValidator>)validator).UnderlyingValidator.MinLength(3);

        // Act
        var result = validator.Validate("ab");

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains("Minimum length", result.Errors[0].Message);
    }

    [Fact]
    public void Validate_WhenInnerValidatorPasses_ReturnsTrue()
    {
        // Arrange
        var validator = Schema.String().WithMessage("String validation failed");
        ((ValidatorWrapper<string, object, StringValidator>)validator).UnderlyingValidator.MinLength(3);

        // Act
        var result = validator.Validate("test");

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_WhenInnerValidatorFails_ReturnsFalse()
    {
        // Arrange
        var validator = Schema.String().WithMessage("String validation failed");
        ((ValidatorWrapper<string, object, StringValidator>)validator).UnderlyingValidator.MinLength(3);

        // Act
        var result = validator.Validate("ab");

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains("Minimum length", result.Errors[0].Message);
    }

    [Fact]
    public void Validate_WithCustomMessage_UsesCustomMessageOnError()
    {
        // Arrange
        var customMessage = "Custom error message";
        var validator = Schema.String().WithMessage(customMessage);
        ((ValidatorWrapper<string, object, StringValidator>)validator).UnderlyingValidator.MinLength(3);

        // Act
        var result = validator.Validate("ab");

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(customMessage, result.Errors[0].Message);
    }

    [Fact]
    public void Validate_WithNestedValidators_ValidatesAllLevels()
    {
        // Arrange
        var validator = UserSchema.Create();
        var user = CreateValidUser() with
        {
            Address = CreateValidAddress()
        };

        // Act
        var result = validator.Validate(user);

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_WithNullValue_HandlesGracefully()
    {
        // Arrange
        var validator = Schema.String().WithMessage("String validation failed");
        ((ValidatorWrapper<string, object, StringValidator>)validator).UnderlyingValidator.MinLength(3);

        // Act
        var result = validator.Validate(null);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains("Expected value of type String", result.Errors[0].Message);
    }

    [Fact]
    public void Constructor_WithNullValidator_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new ValidatorWrapper<string, object, StringValidator>(null));
    }

    [Fact]
    public void Validate_WithMultipleValidationRules_ValidatesAll()
    {
        // Arrange
        var validator = Schema.String().WithMessage("String validation failed");
        var underlyingValidator = ((ValidatorWrapper<string, object, StringValidator>)validator).UnderlyingValidator;
        underlyingValidator.MinLength(3);
        underlyingValidator.MaxLength(10);

        // Act
        var result = validator.Validate("ab");

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains("Minimum length", result.Errors[0].Message);
    }

    [Fact]
    public void Validate_WithValidTypeButInvalidValue_ReturnsFalse()
    {
        // Arrange
        var validator = Schema.String().WithMessage("String validation failed");
        ((ValidatorWrapper<string, object, StringValidator>)validator).UnderlyingValidator.MinLength(3);

        // Act
        var result = validator.Validate("ab");

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains("Minimum length", result.Errors[0].Message);
    }

    [Fact]
    public void Validate_WithComplexObjectHierarchy_ValidatesCorrectly()
    {
        // Arrange
        var validator = UserSchema.Create();
        var user = CreateValidUser() with
        {
            Address = CreateValidAddress()
        };

        // Act
        var result = validator.Validate(user);

        // Assert
        Assert.True(result.IsValid);
    }
} 