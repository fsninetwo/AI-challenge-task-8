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
        _validator = Schema.Array<string>(Schema.String());
        _numberArrayValidator = Schema.Array<double>(Schema.Number());
        _stringArrayValidator = Schema.Array<string>(Schema.String());
        _underlyingNumberValidator = ((SchemaValidation.Core.ValidatorWrapper<IEnumerable<double>, object, ArrayValidator<double>>)_numberArrayValidator).UnderlyingValidator;
        _underlyingStringValidator = ((SchemaValidation.Core.ValidatorWrapper<IEnumerable<string>, object, ArrayValidator<string>>)_stringArrayValidator).UnderlyingValidator;
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
    [InlineData(123)]
    [InlineData("not an array")]
    [InlineData(null)]
    public void Validate_WhenValueIsNotEnumerable_ReturnsFalse(object value)
    {
        // Act
        var result = _numberArrayValidator.Validate(value);

        // Assert
        Assert.False(result.IsValid);
        Assert.NotEmpty(result.Errors);
    }

    [Theory]
    [InlineData(3, 5, 1)]
    [InlineData(3, 5, 7)]
    public void Validate_WhenLengthIsOutOfRange_ReturnsFalse(int minLength, int maxLength, int actualLength)
    {
        // Arrange
        _underlyingNumberValidator.MinLength(minLength);
        _underlyingNumberValidator.MaxLength(maxLength);
        var value = new double[actualLength];
        for (var i = 0; i < actualLength; i++)
        {
            value[i] = i + 1;
        }

        // Act
        var result = _numberArrayValidator.Validate(value);

        // Assert
        Assert.False(result.IsValid);
        Assert.NotEmpty(result.Errors);
    }

    [Fact]
    public void MinLength_WithValidInput_ShouldPass()
    {
        // Arrange
        _underlyingStringValidator.MinLength(2);

        // Act
        var result = _stringArrayValidator.Validate(new[] { "item1", "item2" });

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void MinLength_WithInvalidInput_ShouldFail()
    {
        // Arrange
        _underlyingStringValidator.MinLength(2);

        // Act
        var result = _stringArrayValidator.Validate(new[] { "item1" });

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains("Array must have at least 2 items", result.Errors[0].Message);
    }

    [Fact]
    public void MaxLength_WithValidInput_ShouldPass()
    {
        // Arrange
        _underlyingStringValidator.MaxLength(3);

        // Act
        var result = _stringArrayValidator.Validate(new[] { "item1", "item2" });

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void MaxLength_WithInvalidInput_ShouldFail()
    {
        // Arrange
        _underlyingStringValidator.MaxLength(2);

        // Act
        var result = _stringArrayValidator.Validate(new[] { "item1", "item2", "item3" });

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains("Array must have at most 2 items", result.Errors[0].Message);
    }

    [Fact]
    public void Unique_WithValidInput_ShouldPass()
    {
        // Arrange
        _underlyingStringValidator.Unique();

        // Act
        var result = _stringArrayValidator.Validate(new[] { "item1", "item2", "item3" });

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Unique_WithInvalidInput_ShouldFail()
    {
        // Arrange
        _underlyingStringValidator.Unique();

        // Act
        var result = _stringArrayValidator.Validate(new[] { "item1", "item1", "item2" });

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains("Array contains duplicate items", result.Errors[0].Message);
    }

    [Fact]
    public void UniqueBy_WithValidInput_ShouldPass()
    {
        // Arrange
        _underlyingStringValidator.UniqueBy((x, y) => x.ToLower() == y.ToLower());

        // Act
        var result = _stringArrayValidator.Validate(new[] { "item1", "item2", "item3" });

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void UniqueBy_WithInvalidInput_ShouldFail()
    {
        // Arrange
        _underlyingStringValidator.SetUniqueBy(x => x.ToLower(), "LowercaseValue");

        // Act
        var result = _stringArrayValidator.Validate(new[] { "item1", "ITEM1", "item2" });

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains("Duplicate value", result.Errors[0].Message);
    }

    [Fact]
    public void Validate_WithCustomMessage_UsesCustomMessageOnError()
    {
        // Arrange
        var customMessage = "Array validation failed";
        _underlyingStringValidator.MinLength(3);
        _stringArrayValidator.WithMessage(customMessage);

        // Act
        var result = _stringArrayValidator.Validate(new[] { "item1", "item2" });

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(customMessage, result.Errors[0].Message);
    }

    [Fact]
    public void Validate_WithItemValidator_ValidatesEachItem()
    {
        // Arrange
        var itemValidator = Schema.Number();
        ((SchemaValidation.Core.ValidatorWrapper<double, object, NumberValidator>)itemValidator).UnderlyingValidator.SetMin(0);
        var arrayValidator = Schema.Array<double>(itemValidator);

        // Act
        var result = arrayValidator.Validate(new[] { 1.0, -1.0, 2.0 });

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains("Value must be greater than or equal to 0", result.Errors[0].Message);
    }

    [Fact]
    public void ObjectArrayValidation_ShouldWork()
    {
        // Arrange
        var schema = new Dictionary<string, Validator<object>>
        {
            { nameof(User.Name), Schema.String() },
            { nameof(User.Email), Schema.String() }
        };
        var validator = Schema.ObjectArray<User>(schema);

        var users = new[]
        {
            new User { Id = "1", Name = "John", Email = "john@example.com", Age = 30, IsActive = true },
            new User { Id = "2", Name = "Jane", Email = "jane@example.com", Age = 25, IsActive = true }
        };

        // Act
        var result = validator.Validate(users);

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void NestedArrayValidation_WithValidData_ShouldPass()
    {
        // Arrange
        var innerValidator = Schema.Array<string>(Schema.String());
        var nestedValidator = Schema.Array<IEnumerable<string>>(innerValidator);

        var data = new[]
        {
            new[] { "a", "b" },
            new[] { "c", "d" }
        };

        // Act
        var result = nestedValidator.Validate(data);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void NestedArrayValidation_WithInvalidInnerItem_ShouldFail()
    {
        // Arrange
        var innerValidator = Schema.Array<string>(Schema.String());
        var nestedValidator = Schema.Array<IEnumerable<string>>(innerValidator);

        var data = new[]
        {
            new[] { "a", "b" },
            new string?[] { "c", null }
        };

        // Act
        var result = nestedValidator.Validate(data);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains("Item at index 1", result.Errors[0].Message);
    }

    [Fact]
    public void NonGenericArrayValidator_WithValidItems_ShouldPass()
    {
        // Arrange
        var arrayValidator = new ArrayValidator();
        arrayValidator.SetMinLength(2);
        arrayValidator.SetMaxLength(4);
        arrayValidator.SetItems(Schema.String());

        IEnumerable<object> value = new object[] { "one", "two", "three" };

        // Act
        var result = arrayValidator.Validate(value);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void NonGenericArrayValidator_WithNullItem_ShouldFail()
    {
        // Arrange
        var arrayValidator = new ArrayValidator();
        arrayValidator.SetItems(Schema.String());

        IEnumerable<object?> value = new object?[] { "one", null };

        // Act
        var result = arrayValidator.Validate(value!);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains("cannot be null", result.Errors[0].Message);
    }

    [Fact]
    public void NonGenericArrayValidator_ItemValidatorFailure_ShouldFail()
    {
        var itemValidator = Schema.String().Pattern("^\\d+$");
        var arrayValidator = new ArrayValidator();
        arrayValidator.SetItems(itemValidator);
        IEnumerable<object> value = new object[] { "123", "abc" };
        var result = arrayValidator.Validate(value);
        Assert.False(result.IsValid);
        Assert.Contains("Item at index 1", result.Errors[0].Message);
    }

    [Fact]
    public void NonGenericArrayValidator_MinLengthViolation_ShouldFail()
    {
        var arrayValidator = new ArrayValidator();
        arrayValidator.SetMinLength(3);
        IEnumerable<object> value = new object[] { "a", "b" };
        var result = arrayValidator.Validate(value);
        Assert.False(result.IsValid);
        Assert.Contains("at least", result.Errors[0].Message);
    }

    [Fact]
    public void NonGenericArrayValidator_MaxLengthViolation_ShouldFail()
    {
        var arrayValidator = new ArrayValidator();
        arrayValidator.SetMaxLength(2);
        IEnumerable<object> value = new object[] { "a", "b", "c" };
        var result = arrayValidator.Validate(value);
        Assert.False(result.IsValid);
        Assert.Contains("at most", result.Errors[0].Message);
    }
} 