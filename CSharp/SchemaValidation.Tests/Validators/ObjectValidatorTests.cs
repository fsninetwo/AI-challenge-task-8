using System;
using System.Collections.Generic;
using SchemaValidation.Tests.Base;
using SchemaValidation.Library.Validators;
using SchemaValidation.Core;
using SchemaValidation.Library.Models;
using Xunit;

namespace SchemaValidation.Tests.Validators;

public class ObjectValidatorTests : ValidationTestBase
{
    private readonly ObjectValidator<TestObject> _validator;

    public ObjectValidatorTests()
    {
        var propertyValidators = new Dictionary<string, Validator<object>>
        {
            { nameof(TestObject.StringProperty), Schema.String().WithMessage("String property validation failed") },
            { nameof(TestObject.NumberProperty), Schema.Number().WithMessage("Number property validation failed") },
            { nameof(TestObject.BoolProperty), Schema.Boolean() }
        };
        _validator = new ObjectValidator<TestObject>(propertyValidators);
    }

    private class TestObject
    {
        public string StringProperty { get; set; } = string.Empty;
        public int NumberProperty { get; set; }
        public bool BoolProperty { get; set; }
    }

    [Fact]
    public void RequiredProperties_WithValidInput_ShouldPass()
    {
        // Arrange
        var schema = new Dictionary<string, Validator<object>>
        {
            { nameof(User.Id), Schema.String() },
            { nameof(User.Name), Schema.String() }
        };
        var validator = Schema.Object<User>(schema);

        // Act
        var result = validator.Validate(CreateValidUser());

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void RequiredProperties_WithInvalidInput_ShouldFail()
    {
        // Arrange
        var schema = new Dictionary<string, Validator<object>>
        {
            { nameof(User.Id), Schema.String() },
            { nameof(User.Name), Schema.String() }
        };
        var validator = Schema.Object<User>(schema);

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
        var schema = new Dictionary<string, Validator<object>>
        {
            { nameof(User.Id), Schema.String() },
            { nameof(User.Name), Schema.String() },
            { nameof(User.Email), Schema.String() }
        };
        var validator = Schema.Object<User>(schema);
        validator.Optional(nameof(User.Email));

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
        var schema = new Dictionary<string, Validator<object>>
        {
            { nameof(User.Id), Schema.String() },
            { nameof(User.Name), Schema.String() }
        };
        var validator = Schema.Object<User>(schema).StrictSchema();

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
        var addressSchema = new Dictionary<string, Validator<object>>
        {
            { nameof(Address.Country), Schema.String() }
        };

        var schema = new Dictionary<string, Validator<object>>
        {
            { nameof(User.Id), Schema.String() },
            { nameof(User.Name), Schema.String() },
            { nameof(User.PhoneNumber), Schema.String() },
            { nameof(User.Address), Schema.ObjectAsValidator<Address>(addressSchema) }
        };

        var validator = new ObjectValidator<User>(schema);
        validator.MarkPropertyAsOptional(nameof(User.PhoneNumber));

        validator.AddDependencyRule(
            nameof(User.PhoneNumber),
            nameof(User.PhoneNumber),
            $"{nameof(User.Address)}.{nameof(Address.Country)}",
            (user, phone, country) => country?.ToString() != "USA" || (!string.IsNullOrEmpty(phone?.ToString()) && phone.ToString()!.StartsWith("+1-")));

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
    public void NestedObjectValidation_WithValidTypes_ShouldPass()
    {
        // Arrange
        var addressSchema = new Dictionary<string, Validator<object>>
        {
            { nameof(Address.Street), Schema.String() },
            { nameof(Address.City), Schema.String() },
            { nameof(Address.PostalCode), Schema.String() },
            { nameof(Address.Country), Schema.String() }
        };

        var schema = new Dictionary<string, Validator<object>>
        {
            { nameof(User.Id), Schema.String() },
            { nameof(User.Name), Schema.String() },
            { nameof(User.Address), Schema.ObjectAsValidator<Address>(addressSchema) }
        };

        var validator = Schema.Object<User>(schema);

        // Act
        var result = validator.Validate(CreateValidUser());

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void NestedObjectValidation_WithInvalidTypes_ShouldFail()
    {
        // Arrange
        var addressSchema = new Dictionary<string, Validator<object>>
        {
            { nameof(Address.Street), Schema.String() },
            { nameof(Address.City), Schema.String() },
            { nameof(Address.PostalCode), Schema.String() },
            { nameof(Address.Country), Schema.String() }
        };

        var schema = new Dictionary<string, Validator<object>>
        {
            { nameof(User.Id), Schema.String() },
            { nameof(User.Name), Schema.String() },
            { nameof(User.Address), Schema.ObjectAsValidator<Address>(addressSchema) }
        };

        var validator = Schema.Object<User>(schema);

        // Act
        var result = validator.Validate(CreateInvalidUser());

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName.Contains(nameof(Address.Street)));
    }

    [Fact]
    public void Validate_WhenAllPropertiesAreValid_ReturnsTrue()
    {
        // Arrange
        var testObject = new TestObject
        {
            StringProperty = "test",
            NumberProperty = 42,
            BoolProperty = true
        };

        // Act
        var result = _validator.Validate(testObject);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Validate_WhenStringPropertyTooShort_ReturnsFalse()
    {
        // Arrange
        var testObject = new TestObject
        {
            StringProperty = "",
            NumberProperty = 42,
            BoolProperty = true
        };

        // Act
        var result = _validator.Validate(testObject);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(TestObject.StringProperty));
    }

    [Fact]
    public void Validate_WhenNumberPropertyOutOfRange_ReturnsFalse()
    {
        // Arrange
        var testObject = new TestObject
        {
            StringProperty = "test",
            NumberProperty = -1,
            BoolProperty = true
        };

        // Act
        var result = _validator.Validate(testObject);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(TestObject.NumberProperty));
    }

    [Fact]
    public void Validate_WhenMultiplePropertiesInvalid_ReturnsFalseWithMultipleErrors()
    {
        // Arrange
        var testObject = new TestObject
        {
            StringProperty = "",
            NumberProperty = -1,
            BoolProperty = true
        };

        // Act
        var result = _validator.Validate(testObject);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(TestObject.StringProperty));
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(TestObject.NumberProperty));
    }

    [Fact]
    public void Validate_WhenObjectIsNull_ReturnsFalse()
    {
        // Act
        var result = _validator.Validate(null!);

        // Assert
        Assert.False(result.IsValid);
        Assert.NotEmpty(result.Errors);
    }

    [Fact]
    public void Validate_WithCustomMessage_UsesCustomMessageOnError()
    {
        // Arrange
        var customMessage = "Custom error message";
        var schema = new Dictionary<string, Validator<object>>
        {
            { nameof(User.Id), Schema.String().WithMessage(customMessage) }
        };
        var validator = Schema.Object<User>(schema);

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
        Assert.Contains(result.Errors, e => e.Message == customMessage);
    }
} 