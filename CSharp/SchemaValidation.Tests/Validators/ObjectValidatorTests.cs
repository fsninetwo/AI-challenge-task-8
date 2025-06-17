using System;
using System.Collections.Generic;
using SchemaValidation.Tests.Base;
using SchemaValidation.Validators;
using SchemaValidation.Core;
using SchemaValidation.Models;
using Xunit;
using SchemaValidation.Library.Validators;

namespace SchemaValidation.Tests.Validators;

public class ObjectValidatorTests : ValidationTestBase
{
    private readonly ObjectValidator<TestObject> _validator;

    public ObjectValidatorTests()
    {
        var propertyValidators = new Dictionary<string, Validator<object>>
        {
            { nameof(TestObject.StringProperty), new StringValidator().MinLength(3) },
            { nameof(TestObject.NumberProperty), new NumberValidator().Min(0).Max(100) },
            { nameof(TestObject.BoolProperty), new BooleanValidator() }
        };
        _validator = new ObjectValidator<TestObject>(propertyValidators);
    }

    private class TestObject
    {
        public string StringProperty { get; set; }
        public int NumberProperty { get; set; }
        public bool BoolProperty { get; set; }
    }

    [Fact]
    public void RequiredProperties_WithValidInput_ShouldPass()
    {
        // Arrange
        var validator = new ValidatorWrapper<User, User, ObjectValidator<User>>(
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
    public void RequiredProperties_WithInvalidInput_ShouldFail()
    {
        // Arrange
        var validator = new ValidatorWrapper<User, User, ObjectValidator<User>>(
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
            IsActive = false,
            Tags = new List<string>(),
            Address = new Address
            {
                Street = "",
                City = "",
                PostalCode = "",
                Country = ""
            }
        });

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(User.Id));
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(User.Name));
    }

    [Fact]
    public void OptionalProperties_WithMissingProperties_ShouldPass()
    {
        // Arrange
        var validator = new ValidatorWrapper<User, User, ObjectValidator<User>>(
            new ObjectValidator<User>(new Dictionary<string, Validator<object>>
            {
                { nameof(User.Id), new ValidatorWrapper<string, object, StringValidator>(new StringValidator()) },
                { nameof(User.Name), new ValidatorWrapper<string, object, StringValidator>(new StringValidator()) },
                { nameof(User.Email), new ValidatorWrapper<string, object, StringValidator>(new StringValidator()) }
            }));

        validator.UnderlyingValidator.Optional(nameof(User.Email));

        // Act
        var result = validator.Validate(new User
        {
            Id = "123",
            Name = "John",
            Email = "",
            Age = 0,
            IsActive = false,
            Tags = new List<string>(),
            Address = new Address
            {
                Street = "",
                City = "",
                PostalCode = "",
                Country = ""
            }
        });

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void StrictSchema_WithAdditionalProperties_ShouldFail()
    {
        // Arrange
        var validator = new ValidatorWrapper<User, User, ObjectValidator<User>>(
            new ObjectValidator<User>(new Dictionary<string, Validator<object>>
            {
                { nameof(User.Id), new ValidatorWrapper<string, object, StringValidator>(new StringValidator()) },
                { nameof(User.Name), new ValidatorWrapper<string, object, StringValidator>(new StringValidator()) }
            }).StrictSchema());

        // Act
        var result = validator.Validate(new User
        {
            Id = "123",
            Name = "John",
            Email = "john@example.com",
            Age = 0,
            IsActive = false,
            Tags = new List<string>(),
            Address = new Address
            {
                Street = "",
                City = "",
                PostalCode = "",
                Country = ""
            }
        });

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains("Unknown properties found", result.Errors[0].Message);
    }

    [Fact]
    public void DependencyRule_WithValidInput_ShouldPass()
    {
        // Arrange
        var validator = new ValidatorWrapper<User, User, ObjectValidator<User>>(
            new ObjectValidator<User>(new Dictionary<string, Validator<object>>
            {
                { nameof(User.Id), new ValidatorWrapper<string, object, StringValidator>(new StringValidator()) },
                { nameof(User.Name), new ValidatorWrapper<string, object, StringValidator>(new StringValidator()) },
                { nameof(User.PhoneNumber), new ValidatorWrapper<string, object, StringValidator>(new StringValidator()) },
                { nameof(User.Address), new ValidatorWrapper<Address, object, ObjectValidator<Address>>(
                    new ObjectValidator<Address>(new Dictionary<string, Validator<object>>
                    {
                        { nameof(Address.Country), new ValidatorWrapper<string, object, StringValidator>(new StringValidator()) }
                    })) }
            }));

        validator.UnderlyingValidator.Optional(nameof(User.PhoneNumber));

        validator.UnderlyingValidator
            .When(nameof(User.PhoneNumber), user => user.Address != null && user.Address.Country == "USA")
            .DependsOn<string, string>(
                nameof(User.PhoneNumber),
                $"{nameof(User.Address)}.{nameof(Address.Country)}",
                (phone, country) => country != "USA" || (!string.IsNullOrEmpty(phone) && phone.StartsWith("+1-")),
                "Property 'PhoneNumber' requires a valid phone number starting with '+1-' when the country is USA");

        // Act
        var result = validator.Validate(new User
        {
            Id = "123",
            Name = "John",
            Email = "",
            Age = 0,
            IsActive = false,
            Tags = new List<string>(),
            PhoneNumber = "+1-1234567890",
            Address = new Address
            {
                Street = "",
                City = "",
                PostalCode = "",
                Country = "USA"
            }
        });

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void DependencyRule_WithInvalidInput_ShouldFail()
    {
        // Arrange
        var validator = new ValidatorWrapper<User, User, ObjectValidator<User>>(
            new ObjectValidator<User>(new Dictionary<string, Validator<object>>
            {
                { nameof(User.Id), new ValidatorWrapper<string, object, StringValidator>(new StringValidator()) },
                { nameof(User.Name), new ValidatorWrapper<string, object, StringValidator>(new StringValidator()) },
                { nameof(User.PhoneNumber), new ValidatorWrapper<string, object, StringValidator>(new StringValidator()) },
                { nameof(User.Address), new ValidatorWrapper<Address, object, ObjectValidator<Address>>(
                    new ObjectValidator<Address>(new Dictionary<string, Validator<object>>
                    {
                        { nameof(Address.Country), new ValidatorWrapper<string, object, StringValidator>(new StringValidator()) }
                    })) }
            }));

        validator.UnderlyingValidator.Optional(nameof(User.PhoneNumber));

        validator.UnderlyingValidator
            .When(nameof(User.PhoneNumber), user => user.Address != null && user.Address.Country == "USA")
            .DependsOn<string, string>(
                nameof(User.PhoneNumber),
                $"{nameof(User.Address)}.{nameof(Address.Country)}",
                (phone, country) => country != "USA" || (!string.IsNullOrEmpty(phone) && phone.StartsWith("+1-")),
                "Property 'PhoneNumber' requires a valid phone number starting with '+1-' when the country is USA");

        // Act - Test with null phone number
        var result = validator.Validate(new User
        {
            Id = "123",
            Name = "John",
            Email = "john@example.com",
            Age = 30,
            IsActive = true,
            Tags = new List<string>(),
            PhoneNumber = null,
            Address = new Address
            {
                Street = "123 Main St",
                City = "New York",
                PostalCode = "10001",
                Country = "USA"
            }
        });

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains("Property 'PhoneNumber' requires a valid phone number starting with '+1-' when the country is USA", result.Errors[0].Message);

        // Test with invalid phone number format
        result = validator.Validate(new User
        {
            Id = "123",
            Name = "John",
            Email = "john@example.com",
            Age = 30,
            IsActive = true,
            Tags = new List<string>(),
            PhoneNumber = "1234567890",
            Address = new Address
            {
                Street = "123 Main St",
                City = "New York",
                PostalCode = "10001",
                Country = "USA"
            }
        });

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains("Property 'PhoneNumber' requires a valid phone number starting with '+1-' when the country is USA", result.Errors[0].Message);
    }

    [Fact]
    public void NestedObjectValidation_WithValidTypes_ShouldPass()
    {
        // Arrange
        var validator = new ValidatorWrapper<User, User, ObjectValidator<User>>(
            new ObjectValidator<User>(new Dictionary<string, Validator<object>>
            {
                { nameof(User.Id), new ValidatorWrapper<string, object, StringValidator>(new StringValidator()) },
                { nameof(User.Address), new ValidatorWrapper<Address, object, ObjectValidator<Address>>(
                    new ObjectValidator<Address>(new Dictionary<string, Validator<object>>
                    {
                        { nameof(Address.Street), new ValidatorWrapper<string, object, StringValidator>(new StringValidator()) },
                        { nameof(Address.PostalCode), new ValidatorWrapper<string, object, StringValidator>(new StringValidator().Pattern(@"^\d{5}$")) }
                    })) }
            }));

        // Act
        var result = validator.Validate(new User
        {
            Id = "123",
            Name = "John",
            Email = "john@example.com",
            Age = 30,
            IsActive = true,
            Address = new Address
            {
                Street = "123 Main St",
                City = "New York",
                PostalCode = "12345",
                Country = "USA"
            }
        });

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void NestedObjectValidation_WithInvalidTypes_ShouldFail()
    {
        // Arrange
        var validator = new ValidatorWrapper<User, User, ObjectValidator<User>>(
            new ObjectValidator<User>(new Dictionary<string, Validator<object>>
            {
                { nameof(User.Id), new ValidatorWrapper<string, object, StringValidator>(new StringValidator()) },
                { nameof(User.Address), new ValidatorWrapper<Address, object, ObjectValidator<Address>>(
                    new ObjectValidator<Address>(new Dictionary<string, Validator<object>>
                    {
                        { nameof(Address.Street), new ValidatorWrapper<string, object, StringValidator>(new StringValidator()) },
                        { nameof(Address.PostalCode), new ValidatorWrapper<string, object, StringValidator>(new StringValidator().Pattern(@"^\d{5}$")) }
                    })) }
            }));

        // Act
        var result = validator.Validate(new User
        {
            Id = "123",
            Name = "John",
            Email = "john@example.com",
            Age = 30,
            IsActive = true,
            Address = new Address
            {
                Street = "123 Main St",
                City = "New York",
                PostalCode = "ABC12", // Invalid postal code format
                Country = "USA"
            }
        });

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == $"{nameof(User.Address)}.{nameof(Address.PostalCode)}");
    }

    [Fact]
    public void Validate_WhenAllPropertiesAreValid_ReturnsTrue()
    {
        // Arrange
        var obj = new TestObject
        {
            StringProperty = "test",
            NumberProperty = 50,
            BoolProperty = true
        };

        // Act
        var result = _validator.Validate(obj);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Validate_WhenStringPropertyTooShort_ReturnsFalse()
    {
        // Arrange
        var obj = new TestObject
        {
            StringProperty = "ab",
            NumberProperty = 50,
            BoolProperty = true
        };

        // Act
        var result = _validator.Validate(obj);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(TestObject.StringProperty));
    }

    [Fact]
    public void Validate_WhenNumberPropertyOutOfRange_ReturnsFalse()
    {
        // Arrange
        var obj = new TestObject
        {
            StringProperty = "test",
            NumberProperty = 101,
            BoolProperty = true
        };

        // Act
        var result = _validator.Validate(obj);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(TestObject.NumberProperty));
    }

    [Fact]
    public void Validate_WhenMultiplePropertiesInvalid_ReturnsFalseWithMultipleErrors()
    {
        // Arrange
        var obj = new TestObject
        {
            StringProperty = "ab",
            NumberProperty = 101,
            BoolProperty = true
        };

        // Act
        var result = _validator.Validate(obj);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(TestObject.StringProperty));
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(TestObject.NumberProperty));
    }

    [Fact]
    public void Validate_WhenObjectIsNull_ReturnsFalse()
    {
        // Act
        var result = _validator.Validate(null);

        // Assert
        Assert.False(result.IsValid);
        Assert.NotEmpty(result.Errors);
    }

    [Fact]
    public void Validate_WithCustomMessage_UsesCustomMessageOnError()
    {
        // Arrange
        var customMessage = "Custom validation error";
        _validator.WithMessage(customMessage);

        var obj = new TestObject
        {
            StringProperty = "ab",
            NumberProperty = 101,
            BoolProperty = true
        };

        // Act
        var result = _validator.Validate(obj);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.Message == customMessage);
    }
} 