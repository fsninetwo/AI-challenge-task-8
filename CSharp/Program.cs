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
        // Create a simple user schema
        var userSchema = Schema.Object<User>(new Dictionary<string, Validator<object>>
        {
            { nameof(User.Id), Schema.String().WithMessage("ID must be a string") },
            { nameof(User.Name), Schema.String().MinLength(2).MaxLength(50) },
            { nameof(User.Email), Schema.String().Pattern(@"^[^\s@]+@[^\s@]+\.[^\s@]+$") },
            { nameof(User.Age), Schema.Number() },
            { nameof(User.IsActive), Schema.Boolean() },
            { nameof(User.Tags), Schema.Array(Schema.String()) }
        });

        // Create a valid user
        var validUser = new User
        {
            Id = "12345",
            Name = "John Doe",
            Email = "john@example.com",
            Age = 30,
            IsActive = true,
            Tags = new List<string> { "developer", "designer" }
        };

        // Validate and print result
        var result = userSchema.Validate(validUser);
        Console.WriteLine($"Validation result: {(result.IsValid ? "Valid" : "Invalid")}");
        if (!result.IsValid)
        {
            Console.WriteLine($"Error: {result.ErrorMessage}");
        }
    }
} 