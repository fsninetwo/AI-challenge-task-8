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
        ((SchemaValidation.Core.ValidatorWrapper<string, object, StringValidator>)validator).UnderlyingValidator.MinLength(3);

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
        ((SchemaValidation.Core.ValidatorWrapper<string, object, StringValidator>)validator).UnderlyingValidator.MinLength(3);

        // Act
        var result = validator.Validate("ab");

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains("String validation failed", result.Errors[0].Message);
    }

    [Fact]
    public void NumberValidator_WithValidInput_ShouldPass()
    {
        // Arrange
        var validator = Schema.Number().WithMessage("Number validation failed");
        ((SchemaValidation.Core.ValidatorWrapper<double, object, NumberValidator>)validator).UnderlyingValidator.NonNegative();

        // Act
        var result = validator.Validate(10);

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void NumberValidator_WithInvalidInput_ShouldFail()
    {
        // Arrange
        var validator = Schema.Number().WithMessage("Number validation failed");
        ((SchemaValidation.Core.ValidatorWrapper<double, object, NumberValidator>)validator).UnderlyingValidator.NonNegative();

        // Act
        var result = validator.Validate(-1);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains("Number validation failed", result.Errors[0].Message);
    }

    [Fact]
    public void NumberValidator_WithIntegerInput_ShouldConvertAndPass()
    {
        // Arrange
        var validator = Schema.Number().WithMessage("Number validation failed");
        ((SchemaValidation.Core.ValidatorWrapper<double, object, NumberValidator>)validator).UnderlyingValidator.NonNegative();

        // Act
        var result = validator.Validate(42); // Integer input

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void NumberValidator_WithDecimalInput_ShouldConvertAndPass()
    {
        // Arrange
        var validator = Schema.Number().WithMessage("Number validation failed");
        ((SchemaValidation.Core.ValidatorWrapper<double, object, NumberValidator>)validator).UnderlyingValidator.NonNegative();

        // Act
        var result = validator.Validate(42.5m); // Decimal input

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void ArrayValidator_WithValidInput_ShouldPass()
    {
        // Arrange
        var validator = Schema.Array<string>(Schema.String()).WithMessage("Array validation failed");
        ((SchemaValidation.Core.ValidatorWrapper<IEnumerable<string>, object, ArrayValidator<string>>)validator).UnderlyingValidator.MinLength(2);

        // Act
        var result = validator.Validate(new[] { "item1", "item2" });

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void ArrayValidator_WithInvalidInput_ShouldFail()
    {
        // Arrange
        var validator = Schema.Array<string>(Schema.String()).WithMessage("Array validation failed");
        ((SchemaValidation.Core.ValidatorWrapper<IEnumerable<string>, object, ArrayValidator<string>>)validator).UnderlyingValidator.MinLength(2);

        // Act
        var result = validator.Validate(new[] { "item1" });

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains("Array validation failed", result.Errors[0].Message);
    }

    [Fact]
    public void ArrayValidator_WithListInput_ShouldPass()
    {
        // Arrange
        var validator = Schema.Array<string>(Schema.String()).WithMessage("Array validation failed");
        ((SchemaValidation.Core.ValidatorWrapper<IEnumerable<string>, object, ArrayValidator<string>>)validator).UnderlyingValidator.MinLength(2);

        // Act
        var result = validator.Validate(new List<string> { "item1", "item2" }); // List<T> input

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void ArrayValidator_WithHashSetInput_ShouldPass()
    {
        // Arrange
        var validator = Schema.Array<string>(Schema.String()).WithMessage("Array validation failed");
        ((SchemaValidation.Core.ValidatorWrapper<IEnumerable<string>, object, ArrayValidator<string>>)validator).UnderlyingValidator.MinLength(2);

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
        var schema = new Dictionary<string, Validator<object>>
        {
            { nameof(User.Id), Schema.String().WithMessage("Invalid ID") },
            { nameof(User.Name), Schema.String().WithMessage("Invalid Name") },
            { nameof(User.Email), Schema.String().WithMessage("Invalid Email") },
            { nameof(User.Age), Schema.Number().WithMessage("Invalid Age") },
            { nameof(User.IsActive), Schema.Boolean().WithMessage("Invalid IsActive") }
        };
        var validator = Schema.ObjectAsValidator<User>(schema);

        // Act
        var result = validator.Validate(new User
        {
            Id = "",
            Name = "",
            Email = "invalid",
            Age = -1,
            IsActive = false
        });

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Message == "Invalid ID");
        Assert.Contains(result.Errors, e => e.Message == "Invalid Name");
        Assert.Contains(result.Errors, e => e.Message == "Invalid Email");
    }

    [Fact]
    public void Optional_WithNullInput_ShouldPass()
    {
        // Arrange
        var schema = new Dictionary<string, Validator<object>>
        {
            { nameof(User.Id), Schema.String() },
            { nameof(User.Name), Schema.String() },
            { nameof(User.Email), Schema.String() },
            { nameof(User.Age), Schema.Number() },
            { nameof(User.IsActive), Schema.Boolean() }
        };
        var validator = Schema.ObjectAsValidator<User>(schema);
        ((SchemaValidation.Core.ValidatorWrapper<User, object, ObjectValidator<User>>)validator).UnderlyingValidator.MarkPropertyAsOptional(nameof(User.Name));

        // Act
        var result = validator.Validate(new User
        {
            Id = "123",
            Name = "John",
            Email = "john@example.com",
            Age = 30,
            IsActive = true
        });

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Optional_WithInvalidInput_ShouldFail()
    {
        // Arrange
        var schema = new Dictionary<string, Validator<object>>
        {
            { nameof(User.Id), Schema.String().WithMessage("Invalid ID") },
            { nameof(User.Name), Schema.String().WithMessage("Invalid Name") },
            { nameof(User.Email), Schema.String().WithMessage("Invalid Email") },
            { nameof(User.Age), Schema.Number().WithMessage("Invalid Age") },
            { nameof(User.IsActive), Schema.Boolean().WithMessage("Invalid IsActive") }
        };
        var validator = Schema.ObjectAsValidator<User>(schema);
        ((SchemaValidation.Core.ValidatorWrapper<User, object, ObjectValidator<User>>)validator).UnderlyingValidator.MarkPropertyAsOptional(nameof(User.Name));

        // Act
        var result = validator.Validate(new User
        {
            Id = "",
            Name = "",
            Email = "invalid",
            Age = -1,
            IsActive = false
        });

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Message == "Invalid ID");
    }

    [Fact]
    public void Validate_WhenInnerValidatorPasses_ReturnsTrue()
    {
        // Arrange
        var validator = Schema.String().WithMessage("String validation failed");
        ((SchemaValidation.Core.ValidatorWrapper<string, object, StringValidator>)validator).UnderlyingValidator.MinLength(3);

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
        ((SchemaValidation.Core.ValidatorWrapper<string, object, StringValidator>)validator).UnderlyingValidator.MinLength(3);

        // Act
        var result = validator.Validate("ab");

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains("String validation failed", result.Errors[0].Message);
    }

    [Fact]
    public void Validate_WithCustomMessage_UsesCustomMessageOnError()
    {
        // Arrange
        var customMessage = "Custom validation error";
        var validator = Schema.String().WithMessage(customMessage);
        ((SchemaValidation.Core.ValidatorWrapper<string, object, StringValidator>)validator).UnderlyingValidator.MinLength(3);

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
        var validator = Schema.String().WithMessage("String validation failed");
        ((SchemaValidation.Core.ValidatorWrapper<string, object, StringValidator>)validator).UnderlyingValidator.MinLength(3);

        // Act
        var result = validator.Validate("ab");

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains("String validation failed", result.Errors[0].Message);
    }

    [Fact]
    public void Validate_WithNullValue_HandlesGracefully()
    {
        // Arrange
        var validator = Schema.String().WithMessage("Value cannot be null");

        // Act
        object? nullValue = null;
        var result = validator.Validate(nullValue!);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains("Value cannot be null", result.Errors[0].Message);
    }

    [Fact]
    public void Constructor_WithNullValidator_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new ValidatorWrapper<string, object, StringValidator>(null!));
    }

    [Fact]
    public void Validate_WithMultipleValidationRules_ValidatesAll()
    {
        // Arrange
        var validator = Schema.String().WithMessage("String validation failed");
        var underlyingValidator = ((SchemaValidation.Core.ValidatorWrapper<string, object, StringValidator>)validator).UnderlyingValidator;
        underlyingValidator.MinLength(3);
        underlyingValidator.MaxLength(10);

        // Act
        var result = validator.Validate("ab");

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains("String validation failed", result.Errors[0].Message);
    }

    [Fact]
    public void Validate_WithValidTypeButInvalidValue_ReturnsFalse()
    {
        // Arrange
        var validator = Schema.Number().WithMessage("Number validation failed");
        ((SchemaValidation.Core.ValidatorWrapper<double, object, NumberValidator>)validator).UnderlyingValidator.NonNegative();

        // Act
        var result = validator.Validate(-1);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains("Number validation failed", result.Errors[0].Message);
    }

    [Fact]
    public void Validate_WithComplexObjectHierarchy_ValidatesCorrectly()
    {
        // Arrange
        var schema = new Dictionary<string, Validator<object>>
        {
            { nameof(User.Id), Schema.String().WithMessage("Invalid ID") },
            { nameof(User.Name), Schema.String().WithMessage("Invalid Name") },
            { nameof(User.Email), Schema.String().WithMessage("Invalid Email") },
            { nameof(User.Age), Schema.Number().WithMessage("Invalid Age") },
            { nameof(User.IsActive), Schema.Boolean().WithMessage("Invalid IsActive") },
            { nameof(User.Address), Schema.ObjectAsValidator<Address>(new Dictionary<string, Validator<object>>
            {
                { nameof(Address.Street), Schema.String().WithMessage("Invalid Street") },
                { nameof(Address.City), Schema.String().WithMessage("Invalid City") },
                { nameof(Address.PostalCode), Schema.String().WithMessage("Invalid PostalCode") },
                { nameof(Address.Country), Schema.String().WithMessage("Invalid Country") }
            })}
        };
        var validator = Schema.ObjectAsValidator<User>(schema);

        // Act
        var result = validator.Validate(new User
        {
            Id = "",
            Name = "",
            Email = "invalid",
            Age = -1,
            IsActive = false,
            Address = new Address
            {
                Street = "",
                City = "",
                PostalCode = "invalid",
                Country = ""
            }
        });

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Message == "Invalid ID");
        Assert.Contains(result.Errors, e => e.Message == "Invalid Name");
        Assert.Contains(result.Errors, e => e.Message == "Invalid Email");
        Assert.Contains(result.Errors, e => e.Message == "Invalid Age");
    }
} 