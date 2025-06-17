using System;
using System.Collections.Generic;
using SchemaValidation.Tests.Base;
using SchemaValidation.Library.Validators;
using SchemaValidation.Core;
using SchemaValidation.Library.Models;
using SchemaValidation.Library.Schemas;
using Xunit;

namespace SchemaValidation.Tests.Validators;

public class ArrayValidatorTests : ValidationTestBase
{
    private readonly Validator<object> _validator;
    private readonly Validator<object> _numberArrayValidator;
    private readonly Validator<object> _stringArrayValidator;
    private readonly ArrayValidator<double> _underlyingNumberValidator;
    private readonly ArrayValidator<string> _underlyingStringValidator;

    public ArrayValidatorTests()
    {
        _validator = Schema.Array(Schema.String());
        _numberArrayValidator = Schema.Array(Schema.Number());
        _stringArrayValidator = Schema.Array(Schema.String());
        _underlyingNumberValidator = ((ValidatorWrapper<IEnumerable<double>, object, ArrayValidator<double>>)_numberArrayValidator).UnderlyingValidator;
        _underlyingStringValidator = ((ValidatorWrapper<IEnumerable<string>, object, ArrayValidator<string>>)_stringArrayValidator).UnderlyingValidator;
    }

    [Fact]
    public void Validate_WhenValueIsArray_ReturnsTrue()
    {
        // Arrange
        var value = new[] { 1.0, 2.0, 3.0 };

        // Act
        var result = _numberArrayValidator.Validate(value);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Validate_WhenValueIsList_ReturnsTrue()
    {
        // Arrange
        var value = new List<double> { 1.0, 2.0, 3.0 };

        // Act
        var result = _numberArrayValidator.Validate(value);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Theory]
    [InlineData("not an array")]
    [InlineData(123)]
    [InlineData(null)]
    public void Validate_WhenValueIsNotEnumerable_ReturnsFalse(object value)
    {
        // Act
        var result = _validator.Validate(value);

        // Assert
        Assert.False(result.IsValid);
        Assert.NotEmpty(result.Errors);
    }

    [Theory]
    [InlineData(3, 5)]
    [InlineData(1, 1)]
    public void Validate_WhenLengthIsInRange_ReturnsTrue(int minLength, int maxLength)
    {
        // Arrange
        var value = new List<double>();
        for (int i = 0; i < minLength; i++)
        {
            value.Add(i);
        }

        _underlyingNumberValidator.MinLength(minLength);
        _underlyingNumberValidator.MaxLength(maxLength);

        // Act
        var result = _numberArrayValidator.Validate(value);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Theory]
    [InlineData(3, 5, 1)]  // Too few items
    [InlineData(3, 5, 7)]  // Too many items
    public void Validate_WhenLengthIsOutOfRange_ReturnsFalse(int minLength, int maxLength, int actualLength)
    {
        // Arrange
        var value = new List<double>();
        for (int i = 0; i < actualLength; i++)
        {
            value.Add(i);
        }

        _underlyingNumberValidator.MinLength(minLength);
        _underlyingNumberValidator.MaxLength(maxLength);

        // Act
        var result = _numberArrayValidator.Validate(value);

        // Assert
        Assert.False(result.IsValid);
        Assert.NotEmpty(result.Errors);
    }

    [Fact]
    public void Validate_WithCustomMessage_UsesCustomMessageOnError()
    {
        // Arrange
        var value = new List<double>();
        var customMessage = "Array must have at least 3 items";

        _underlyingNumberValidator.MinLength(3).WithMessage(customMessage);

        // Act
        var result = _numberArrayValidator.Validate(value);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.Message == customMessage);
    }

    [Fact]
    public void Validate_WithItemValidator_ValidatesEachItem()
    {
        // Arrange
        var numberValidator = new NumberValidator().Min(0).Max(10);
        var arrayValidator = new ArrayValidator<double>(numberValidator);

        var validValue = new[] { 1.0, 5.0, 10.0 };
        var invalidValue = new[] { -1.0, 5.0, 11.0 };

        // Act
        var validResult = arrayValidator.Validate(validValue);
        var invalidResult = arrayValidator.Validate(invalidValue);

        // Assert
        Assert.True(validResult.IsValid);
        Assert.Empty(validResult.Errors);

        Assert.False(invalidResult.IsValid);
        Assert.NotEmpty(invalidResult.Errors);
    }

    [Fact]
    public void MinLength_WithValidInput_ShouldPass()
    {
        // Arrange
        var validator = Schema.Array(Schema.String()).WithMessage("Array validation failed");
        var underlyingValidator = ((ValidatorWrapper<IEnumerable<string>, object, ArrayValidator<string>>)validator).UnderlyingValidator;
        underlyingValidator.MinLength(2);

        // Act
        var result = validator.Validate(new[] { "item1", "item2" });

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void MinLength_WithInvalidInput_ShouldFail()
    {
        // Arrange
        var validator = Schema.Array(Schema.String()).WithMessage("Array validation failed");
        var underlyingValidator = ((ValidatorWrapper<IEnumerable<string>, object, ArrayValidator<string>>)validator).UnderlyingValidator;
        underlyingValidator.MinLength(2);

        // Act
        var result = validator.Validate(new[] { "item1" });

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains("Array must have at least 2 items", result.Errors[0].Message);
    }

    [Fact]
    public void MaxLength_WithValidInput_ShouldPass()
    {
        // Arrange
        var validator = Schema.Array(Schema.String()).WithMessage("Array validation failed");
        var underlyingValidator = ((ValidatorWrapper<IEnumerable<string>, object, ArrayValidator<string>>)validator).UnderlyingValidator;
        underlyingValidator.MaxLength(3);

        // Act
        var result = validator.Validate(new[] { "item1", "item2", "item3" });

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void MaxLength_WithInvalidInput_ShouldFail()
    {
        // Arrange
        var validator = Schema.Array(Schema.String()).WithMessage("Array validation failed");
        var underlyingValidator = ((ValidatorWrapper<IEnumerable<string>, object, ArrayValidator<string>>)validator).UnderlyingValidator;
        underlyingValidator.MaxLength(2);

        // Act
        var result = validator.Validate(new[] { "item1", "item2", "item3" });

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains("Array must have at most 2 items", result.Errors[0].Message);
    }

    [Fact]
    public void Unique_WithValidInput_ShouldPass()
    {
        // Arrange
        var validator = Schema.Array(Schema.String()).WithMessage("Array validation failed");
        var underlyingValidator = ((ValidatorWrapper<IEnumerable<string>, object, ArrayValidator<string>>)validator).UnderlyingValidator;
        underlyingValidator.Unique();

        // Act
        var result = validator.Validate(new[] { "item1", "item2", "item3" });

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Unique_WithInvalidInput_ShouldFail()
    {
        // Arrange
        var validator = Schema.Array(Schema.String()).WithMessage("Array validation failed");
        var underlyingValidator = ((ValidatorWrapper<IEnumerable<string>, object, ArrayValidator<string>>)validator).UnderlyingValidator;
        underlyingValidator.Unique();

        // Act
        var result = validator.Validate(new[] { "item1", "item1", "item2" });

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains("Array contains duplicate items", result.Errors[0].Message);
    }

    [Fact]
    public void UniqueBy_WithValidInput_ShouldPass()
    {
        // Arrange
        var validator = Schema.Array(Schema.String()).WithMessage("Array validation failed");
        var underlyingValidator = ((ValidatorWrapper<IEnumerable<string>, object, ArrayValidator<string>>)validator).UnderlyingValidator;
        underlyingValidator.UniqueBy((x, y) => x.Length == y.Length);

        // Act
        var result = validator.Validate(new[] { "a", "bb", "ccc" });

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void UniqueBy_WithInvalidInput_ShouldFail()
    {
        // Arrange
        var validator = Schema.Array(Schema.String()).WithMessage("Array validation failed");
        var underlyingValidator = ((ValidatorWrapper<IEnumerable<string>, object, ArrayValidator<string>>)validator).UnderlyingValidator;
        underlyingValidator.UniqueBy((x, y) => x.Length == y.Length);

        // Act
        var result = validator.Validate(new[] { "a", "b", "c" });

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains("Array contains items that are considered duplicates by the custom comparer", result.Errors[0].Message);
    }

    [Fact]
    public void ObjectArrayValidation_ShouldWork()
    {
        // Arrange
        var schema = new Dictionary<string, Validator<object>>
        {
            { nameof(User.Name), Schema.String().WithMessage("Name validation failed") },
            { nameof(User.Age), Schema.Number().WithMessage("Age validation failed") }
        };
        var validator = Schema.ObjectArray<User>(schema);

        var validValue = new[]
        {
            CreateValidUser(),
            CreateValidUser() with { Id = "456", Name = "Jane Smith", Email = "jane@example.com" }
        };

        // Act
        var result = validator.Validate(validValue);

        // Assert
        Assert.True(result.IsValid);
    }
} 