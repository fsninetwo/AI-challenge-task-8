using System;
using System.Collections.Generic;
using SchemaValidation.Core;
using SchemaValidation.Models;
using SchemaValidation.Validators;

namespace SchemaValidation;

public static class Program
{
    public static void Main()
    {
        // Define address schema
        var addressSchema = Schema.Object<Address>(new Dictionary<string, Validator<object>>
        {
            { nameof(Address.Street), Schema.String() },
            { nameof(Address.City), Schema.String() },
            { nameof(Address.PostalCode), Schema.String().Pattern(@"^\d{5}$").WithMessage("Postal code must be 5 digits") },
            { nameof(Address.Country), Schema.String() }
        });

        // Define user schema
        var userSchema = Schema.Object<User>(new Dictionary<string, Validator<object>>
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
        userSchema.Optional(nameof(User.PhoneNumber));

        // Add condition: postal code is required only for US addresses
        userSchema
            .When(nameof(User.Address), user => (user.Address?.Country ?? "").Equals("USA", StringComparison.OrdinalIgnoreCase))
            .DependsOn(nameof(User.Address), nameof(User.PhoneNumber), (address, phone) => 
                address is Address addr && 
                addr.Country.Equals("USA", StringComparison.OrdinalIgnoreCase) && 
                phone is not null);

        Console.WriteLine("Test Case 1: Valid User");
        var validUser = new User
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
        ValidateAndPrint(userSchema, validUser);

        Console.WriteLine("\nTest Case 2: Invalid Email");
        var invalidEmailUser = new User
        {
            Id = "12345",
            Name = "John Doe",
            Email = "invalid-email",
            Age = 30,
            IsActive = true,
            Tags = new List<string> { "developer", "designer" },
            Address = validUser.Address,
            PhoneNumber = "+1-1234567890"
        };
        ValidateAndPrint(userSchema, invalidEmailUser);

        Console.WriteLine("\nTest Case 3: Invalid Name Length");
        var invalidNameUser = new User
        {
            Id = "12345",
            Name = "J",  // Too short
            Email = "john@example.com",
            Age = 30,
            IsActive = true,
            Tags = new List<string> { "developer", "designer" },
            Address = validUser.Address,
            PhoneNumber = "+1-1234567890"
        };
        ValidateAndPrint(userSchema, invalidNameUser);

        Console.WriteLine("\nTest Case 4: Invalid Postal Code");
        var invalidAddress = new Address
        {
            Street = "123 Main St",
            City = "Anytown",
            PostalCode = "123",  // Not 5 digits
            Country = "USA"
        };
        ValidateAndPrint(addressSchema, invalidAddress);
    }

    private static void ValidateAndPrint<T>(Validator<T> schema, T data)
    {
        ArgumentNullException.ThrowIfNull(schema);
        ArgumentNullException.ThrowIfNull(data);

        var result = schema.Validate(data);
        Console.WriteLine($"Validation result: {(result.IsValid ? "Valid" : "Invalid")}");
        if (!result.IsValid)
        {
            Console.WriteLine($"Error: {result.ErrorMessage}");
        }
    }
} 