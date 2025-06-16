using System;
using System.Collections.Generic;
using Xunit;
using SchemaValidation.Core;
using SchemaValidation.Models;
using SchemaValidation.Validators;

namespace SchemaValidation.Tests;

public class SchemaValidationTests
{
    private readonly Validator<User> _userSchema;
    private readonly Validator<Address> _addressSchema;

    public SchemaValidationTests()
    {
        // Define address schema
        _addressSchema = Schema.Object<Address>(new Dictionary<string, Validator<object>>
        {
            { nameof(Address.Street), Schema.String() },
            { nameof(Address.City), Schema.String() },
            { nameof(Address.PostalCode), Schema.String().Pattern(@"^\d{5}$").WithMessage("Postal code must be 5 digits") },
            { nameof(Address.Country), Schema.String() }
        });

        // Define user schema
        _userSchema = Schema.Object<User>(new Dictionary<string, Validator<object>>
        {
            { nameof(User.Id), Schema.String().WithMessage("ID must be a string") },
            { nameof(User.Name), Schema.String().MinLength(2).MaxLength(50) },
            { nameof(User.Email), Schema.String().Pattern(@"^[^\s@]+@[^\s@]+\.[^\s@]+$") },
            { nameof(User.Age), Schema.Number() },
            { nameof(User.IsActive), Schema.Boolean() },
            { nameof(User.Tags), Schema.Array(Schema.String()) },
            { nameof(User.PhoneNumber), Schema.String().Pattern(@"^\+\d{1,3}-\d{3,14}$") },
            { nameof(User.Address), Schema.ObjectAsValidator<Address>(new Dictionary<string, Validator<object>>
            {
                { nameof(Address.Street), Schema.String() },
                { nameof(Address.City), Schema.String() },
                { nameof(Address.PostalCode), Schema.String().Pattern(@"^\d{5}(-\d{4})?$") },
                { nameof(Address.Country), Schema.String() }
            }) }
        });

        // Make phone number optional
        if (_userSchema is ObjectValidator<User> userValidator)
        {
            userValidator.MarkPropertyAsOptional(nameof(User.PhoneNumber));

            // Add condition: postal code is required only for US addresses
            userValidator
                .When(nameof(User.Address), user => (user.Address?.Country ?? "").Equals("USA", StringComparison.OrdinalIgnoreCase))
                .DependsOn<Address, string>(nameof(User.Address), nameof(User.PhoneNumber), (address, phone) => 
                    address.Country.Equals("USA", StringComparison.OrdinalIgnoreCase) && 
                    phone is not null);
        }
    }

    [Fact]
    public void ValidUser_ShouldPass()
    {
        // Arrange
        var user = new User
        {
            Id = "12345",
            Name = "John Doe",
            Email = "john@example.com",
            Age = 30,
            IsActive = true,
            Tags = new List<string> { "developer", "designer" },
            Address = new Address
            {
                Street = "123 Main St",
                City = "Anytown",
                PostalCode = "12345",
                Country = "USA"
            },
            PhoneNumber = "+1-1234567890"
        };

        // Act
        var result = _userSchema.Validate(user);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void InvalidEmail_ShouldFail()
    {
        // Arrange
        var user = new User
        {
            Id = "12345",
            Name = "John Doe",
            Email = "invalid-email",
            Age = 30,
            IsActive = true,
            Tags = new List<string> { "developer", "designer" },
            Address = new Address
            {
                Street = "123 Main St",
                City = "Anytown",
                PostalCode = "12345",
                Country = "USA"
            },
            PhoneNumber = "+1-1234567890"
        };

        // Act
        var result = _userSchema.Validate(user);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(User.Email));
        Assert.Contains("Pattern validation", result.Errors[0].Message);
    }

    [Fact]
    public void InvalidNameLength_ShouldFail()
    {
        // Arrange
        var user = new User
        {
            Id = "12345",
            Name = "J", // Too short
            Email = "john@example.com",
            Age = 30,
            IsActive = true,
            Tags = new List<string> { "developer", "designer" },
            Address = new Address
            {
                Street = "123 Main St",
                City = "Anytown",
                PostalCode = "12345",
                Country = "USA"
            },
            PhoneNumber = "+1-1234567890"
        };

        // Act
        var result = _userSchema.Validate(user);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(User.Name));
        Assert.Contains("Minimum length", result.Errors[0].Message);
    }

    [Fact]
    public void InvalidPostalCode_ShouldFail()
    {
        // Arrange
        var address = new Address
        {
            Street = "123 Main St",
            City = "Anytown",
            PostalCode = "123", // Not 5 digits
            Country = "USA"
        };

        // Act
        var result = _addressSchema.Validate(address);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(Address.PostalCode));
        Assert.Contains("Pattern validation", result.Errors[0].Message);
    }

    [Fact]
    public void NonUSAddress_WithoutPhoneNumber_ShouldPass()
    {
        // Arrange
        var user = new User
        {
            Id = "12345",
            Name = "John Doe",
            Email = "john@example.com",
            Age = 30,
            IsActive = true,
            Tags = new List<string> { "developer", "designer" },
            Address = new Address
            {
                Street = "123 Main St",
                City = "Toronto",
                PostalCode = "12345",
                Country = "Canada"
            }
            // No phone number for non-US address
        };

        // Act
        var result = _userSchema.Validate(user);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void USAddress_WithoutPhoneNumber_ShouldFail()
    {
        // Arrange
        var user = new User
        {
            Id = "12345",
            Name = "John Doe",
            Email = "john@example.com",
            Age = 30,
            IsActive = true,
            Tags = new List<string> { "developer", "designer" },
            Address = new Address
            {
                Street = "123 Main St",
                City = "Anytown",
                PostalCode = "12345",
                Country = "USA"
            }
            // Missing phone number for US address
        };

        // Act
        var result = _userSchema.Validate(user);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(User.Address));
        Assert.Contains("requires a valid phone number", result.Errors[0].Message.ToLower());
    }
} 