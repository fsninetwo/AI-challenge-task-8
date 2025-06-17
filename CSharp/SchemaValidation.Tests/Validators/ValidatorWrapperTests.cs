using System;
using System.Collections.Generic;
using SchemaValidation.Tests.Base;
using SchemaValidation.Library.Validators;
using SchemaValidation.Core;
using SchemaValidation.Models;
using Xunit;

namespace SchemaValidation.Tests.Validators;

public class ValidatorWrapperTests : ValidationTestBase
{
    [Fact]
    public void StringValidator_WithValidInput_ShouldPass()
    {
        // Arrange
        var validator = new ValidatorWrapper<string, object, StringValidator>(
            new StringValidator().MinLength(3));

        // Act
        var result = validator.Validate("test");

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void StringValidator_WithInvalidInput_ShouldFail()
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
    public void NumberValidator_WithValidInput_ShouldPass()
    {
        // Arrange
        var validator = new ValidatorWrapper<double, object, NumberValidator>(
            new NumberValidator().NonNegative());

        // Act
        var result = validator.Validate(10);

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void NumberValidator_WithInvalidInput_ShouldFail()
    {
        // Arrange
        var validator = new ValidatorWrapper<double, object, NumberValidator>(
            new NumberValidator().NonNegative());

        // Act
        var result = validator.Validate(-1);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains("must be non-negative", result.Errors[0].Message);
    }

    [Fact]
    public void NumberValidator_WithIntegerInput_ShouldConvertAndPass()
    {
        // Arrange
        var validator = new ValidatorWrapper<double, object, NumberValidator>(
            new NumberValidator().NonNegative());

        // Act
        var result = validator.Validate(42); // Integer input

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void NumberValidator_WithDecimalInput_ShouldConvertAndPass()
    {
        // Arrange
        var validator = new ValidatorWrapper<double, object, NumberValidator>(
            new NumberValidator().NonNegative());

        // Act
        var result = validator.Validate(42.5m); // Decimal input

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void ArrayValidator_WithValidInput_ShouldPass()
    {
        // Arrange
        var validator = new ValidatorWrapper<IEnumerable<string>, object, ArrayValidator<string>>(
            new ArrayValidator<string>(new StringValidator()).MinLength(2));

        // Act
        var result = validator.Validate(new[] { "item1", "item2" });

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void ArrayValidator_WithInvalidInput_ShouldFail()
    {
        // Arrange
        var validator = new ValidatorWrapper<IEnumerable<string>, object, ArrayValidator<string>>(
            new ArrayValidator<string>(new StringValidator()).MinLength(2));

        // Act
        var result = validator.Validate(new[] { "item1" });

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains("Array must contain at least 2 items", result.Errors[0].Message);
    }

    [Fact]
    public void ArrayValidator_WithListInput_ShouldPass()
    {
        // Arrange
        var validator = new ValidatorWrapper<IEnumerable<string>, object, ArrayValidator<string>>(
            new ArrayValidator<string>(new StringValidator()).MinLength(2));

        // Act
        var result = validator.Validate(new List<string> { "item1", "item2" }); // List<T> input

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void ArrayValidator_WithHashSetInput_ShouldPass()
    {
        // Arrange
        var validator = new ValidatorWrapper<IEnumerable<string>, object, ArrayValidator<string>>(
            new ArrayValidator<string>(new StringValidator()).MinLength(2));

        // Act
        var result = validator.Validate(new HashSet<string> { "item1", "item2" }); // HashSet<T> input

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void ObjectValidator_WithValidInput_ShouldPass()
    {
        // Arrange
        var validator = new ValidatorWrapper<User, object, ObjectValidator<User>>(
            new ObjectValidator<User>(new Dictionary<string, Validator<object>>
            {
                { nameof(User.Id), new ValidatorWrapper<string, object, StringValidator>(new StringValidator()) },
                { nameof(User.Name), new ValidatorWrapper<string, object, StringValidator>(new StringValidator()) }
            }));

        // Act
        var result = validator.Validate(CreateValidUser());

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void ObjectValidator_WithInvalidInput_ShouldFail()
    {
        // Arrange
        var validator = new ValidatorWrapper<User, object, ObjectValidator<User>>(
            new ObjectValidator<User>(new Dictionary<string, Validator<object>>
            {
                { nameof(User.Id), new ValidatorWrapper<string, object, StringValidator>(new StringValidator()) },
                { nameof(User.Name), new ValidatorWrapper<string, object, StringValidator>(new StringValidator()) }
            }));

        // Act
        var result = validator.Validate(new User
        {
            Id = "",
            Name = "",
            Email = "",
            Age = 0,
            IsActive = false
        });

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(User.Id));
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(User.Name));
    }

    [Fact]
    public void Optional_WithNullInput_ShouldPass()
    {
        // Arrange
        var validator = new ValidatorWrapper<User, object, ObjectValidator<User>>(
            new ObjectValidator<User>(new Dictionary<string, Validator<object>>
            {
                { nameof(User.Id), new ValidatorWrapper<string, object, StringValidator>(new StringValidator()) },
                { nameof(User.Name), new ValidatorWrapper<string, object, StringValidator>(new StringValidator()) }
            }));

        validator.UnderlyingValidator.Optional(nameof(User.Name));

        // Act
        var result = validator.Validate(new User
        {
            Id = "123",
            Name = null!,
            Email = "",
            Age = 0,
            IsActive = false
        });

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Optional_WithInvalidInput_ShouldFail()
    {
        // Arrange
        var validator = new ValidatorWrapper<User, object, ObjectValidator<User>>(
            new ObjectValidator<User>(new Dictionary<string, Validator<object>>
            {
                { nameof(User.Id), new ValidatorWrapper<string, object, StringValidator>(new StringValidator()) },
                { nameof(User.Name), new ValidatorWrapper<string, object, StringValidator>(new StringValidator().MinLength(3)) }
            }));

        validator.UnderlyingValidator.Optional(nameof(User.Name));

        // Act
        var result = validator.Validate(new User
        {
            Id = "123",
            Name = "ab",
            Email = "",
            Age = 0,
            IsActive = false
        });

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains("Minimum length", result.Errors[0].Message);
    }

    [Fact]
    public void Validate_WhenInnerValidatorPasses_ReturnsTrue()
    {
        // Arrange
        var stringValidator = new StringValidator().MinLength(3);
        var wrapper = new ValidatorWrapper<string, object, StringValidator>(stringValidator);

        // Act
        var result = wrapper.Validate("test");

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Validate_WhenInnerValidatorFails_ReturnsFalse()
    {
        // Arrange
        var stringValidator = new StringValidator().MinLength(3);
        var wrapper = new ValidatorWrapper<string, object, StringValidator>(stringValidator);

        // Act
        var result = wrapper.Validate("ab");

        // Assert
        Assert.False(result.IsValid);
        Assert.NotEmpty(result.Errors);
    }

    [Fact]
    public void Validate_WithCustomMessage_UsesCustomMessageOnError()
    {
        // Arrange
        var customMessage = "Custom validation error";
        var stringValidator = new StringValidator().MinLength(3).WithMessage(customMessage);
        var wrapper = new ValidatorWrapper<string, object, StringValidator>(stringValidator);

        // Act
        var result = wrapper.Validate("ab");

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.Message == customMessage);
    }

    [Fact]
    public void Validate_WithNestedValidators_ValidatesAllLevels()
    {
        // Arrange
        var addressValidators = new Dictionary<string, Validator<object>>
        {
            { nameof(Address.Street), new ValidatorWrapper<string, object, StringValidator>(new StringValidator().MinLength(5)) },
            { nameof(Address.PostalCode), new ValidatorWrapper<string, object, StringValidator>(new StringValidator().Pattern(@"^\d{5}$")) }
        };

        var userValidators = new Dictionary<string, Validator<object>>
        {
            { nameof(User.Name), new ValidatorWrapper<string, object, StringValidator>(new StringValidator().MinLength(2)) },
            { nameof(User.Age), new ValidatorWrapper<double, object, NumberValidator>(new NumberValidator().Min(0).Max(120)) },
            { nameof(User.Address), new ValidatorWrapper<Address, object, ObjectValidator<Address>>(new ObjectValidator<Address>(addressValidators)) }
        };

        var wrapper = new ValidatorWrapper<User, object, ObjectValidator<User>>(new ObjectValidator<User>(userValidators));

        var user = CreateInvalidUser();

        // Act
        var result = wrapper.Validate(user);

        // Assert
        Assert.False(result.IsValid);
        Assert.True(result.Errors.Count >= 4); // Should have errors for all invalid fields
    }

    [Fact]
    public void Validate_WithNullValue_HandlesGracefully()
    {
        // Arrange
        var stringValidator = new StringValidator().MinLength(3);
        var wrapper = new ValidatorWrapper<string, object, StringValidator>(stringValidator);

        // Act
        var result = wrapper.Validate(null);

        // Assert
        Assert.False(result.IsValid);
        Assert.NotEmpty(result.Errors);
    }

    [Fact]
    public void Constructor_WithNullValidator_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            new ValidatorWrapper<string, object, StringValidator>(null));
    }

    [Fact]
    public void Validate_WithMultipleValidationRules_ValidatesAll()
    {
        // Arrange
        var stringValidator = new StringValidator()
            .MinLength(3)
            .MaxLength(10)
            .Pattern(@"^[a-zA-Z]+$");
        var wrapper = new ValidatorWrapper<string, object, StringValidator>(stringValidator);

        // Act
        var result = wrapper.Validate("ab123");

        // Assert
        Assert.False(result.IsValid);
        Assert.NotEmpty(result.Errors);
    }

    [Fact]
    public void Validate_WithValidTypeButInvalidValue_ReturnsFalse()
    {
        // Arrange
        var numberValidator = new NumberValidator().Min(0).Max(100);
        var wrapper = new ValidatorWrapper<double, object, NumberValidator>(numberValidator);

        // Act
        var result = wrapper.Validate(-1);

        // Assert
        Assert.False(result.IsValid);
        Assert.NotEmpty(result.Errors);
    }

    [Fact]
    public void Validate_WithComplexObjectHierarchy_ValidatesCorrectly()
    {
        // Arrange
        var addressValidators = new Dictionary<string, Validator<object>>
        {
            { nameof(Address.Street), new ValidatorWrapper<string, object, StringValidator>(new StringValidator().MinLength(5)) },
            { nameof(Address.PostalCode), new ValidatorWrapper<string, object, StringValidator>(new StringValidator().Pattern(@"^\d{5}$")) },
            { nameof(Address.City), new ValidatorWrapper<string, object, StringValidator>(new StringValidator().MinLength(2)) },
            { nameof(Address.Country), new ValidatorWrapper<string, object, StringValidator>(new StringValidator().MinLength(2)) }
        };

        var userValidators = new Dictionary<string, Validator<object>>
        {
            { nameof(User.Name), new ValidatorWrapper<string, object, StringValidator>(new StringValidator().MinLength(2)) },
            { nameof(User.Age), new ValidatorWrapper<double, object, NumberValidator>(new NumberValidator().Min(0).Max(120)) },
            { nameof(User.Email), new ValidatorWrapper<string, object, StringValidator>(new StringValidator().Pattern(@"^[^@\s]+@[^@\s]+\.[^@\s]+$")) },
            { nameof(User.Id), new ValidatorWrapper<string, object, StringValidator>(new StringValidator().MinLength(1)) },
            { nameof(User.Address), new ValidatorWrapper<Address, object, ObjectValidator<Address>>(new ObjectValidator<Address>(addressValidators)) }
        };

        var wrapper = new ValidatorWrapper<User, object, ObjectValidator<User>>(new ObjectValidator<User>(userValidators));

        var user = CreateInvalidUser();

        // Act
        var result = wrapper.Validate(user);

        // Assert
        Assert.False(result.IsValid);
        Assert.True(result.Errors.Count >= 5); // Should have errors for all invalid fields
    }

    private class User
    {
        public string Name { get; set; }
        public int Age { get; set; }
        public Address Address { get; set; }
    }

    private class Address
    {
        public string Street { get; set; }
        public string PostalCode { get; set; }
    }
} 