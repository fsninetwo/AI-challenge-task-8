using Microsoft.Extensions.DependencyInjection;
using SchemaValidation.Core;
using SchemaValidation.Library.Models;
using SchemaValidation.Library.Validators;
using System.Collections.Generic;

class Program
{
    static void Main(string[] args)
    {
        // Set up dependency injection
        var services = new ServiceCollection();
        ConfigureServices(services);
        var serviceProvider = services.BuildServiceProvider();

        // Get the validator from DI container
        var validator = serviceProvider.GetRequiredService<ObjectValidator<User>>();

        // Demo 1: Valid User (USA)
        Console.WriteLine("Demo 1: Valid User (USA)");
        var validUserUSA = new User
        {
            Id = "123",
            Name = "John Doe",
            Email = "john.doe@example.com",
            Age = 30,
            IsActive = true,
            Tags = new List<string> { "developer", "admin" },
            PhoneNumber = "+1-1234567890",
            Address = new Address
            {
                Street = "123 Main Street",
                City = "New York",
                PostalCode = "12345",
                Country = "USA"
            }
        };
        var validResultUSA = validator.Validate(validUserUSA);
        PrintValidationResult(validResultUSA);

        // Demo 2: Valid User (Non-USA, optional phone)
        Console.WriteLine("\nDemo 2: Valid User (Non-USA)");
        var validUserNonUSA = new User
        {
            Id = "456",
            Name = "Jane Smith",
            Email = "jane.smith@example.com",
            Age = 25,
            IsActive = true,
            Tags = new List<string> { "designer" },
            PhoneNumber = "+44-1234567890",
            Address = new Address
            {
                Street = "456 High Street",
                City = "London",
                PostalCode = "12345",
                Country = "UK"
            }
        };
        var validResultNonUSA = validator.Validate(validUserNonUSA);
        PrintValidationResult(validResultNonUSA);

        // Demo 3: Invalid User
        Console.WriteLine("\nDemo 3: Invalid User");
        var invalidUser = new User
        {
            Id = "",
            Name = "J",  // Too short
            Email = "invalid-email",
            Age = -5,
            IsActive = false,
            Tags = new List<string>(),  // Empty array
            PhoneNumber = "invalid-phone",
            Address = new Address
            {
                Street = "1 St",  // Too short
                City = "",
                PostalCode = "ABC",  // Invalid format
                Country = "USA"  // Requires +1 phone for USA
            }
        };
        var invalidResult = validator.Validate(invalidUser);
        PrintValidationResult(invalidResult);
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        // Register the User validator
        services.AddSingleton<ObjectValidator<User>>(provider =>
        {
            var addressSchema = new Dictionary<string, Validator<object>>
            {
                { nameof(Address.Street), Schema.String().MinLength(5).WithMessage("Street must be at least 5 characters long") },
                { nameof(Address.City), Schema.String().WithMessage("City is required") },
                { nameof(Address.PostalCode), Schema.String().Pattern(@"^\d{5}$").WithMessage("Postal code must be exactly 5 digits") },
                { nameof(Address.Country), Schema.String().WithMessage("Country is required") }
            };

            var schema = new Dictionary<string, Validator<object>>
            {
                { nameof(User.Id), Schema.String().MinLength(1).WithMessage("Id is required") },
                { nameof(User.Name), Schema.String().MinLength(2).WithMessage("Name must be at least 2 characters long") },
                { nameof(User.Email), Schema.String().Pattern(@"^[^\s@]+@[^\s@]+\.[^\s@]+$").WithMessage("Email must be a valid email address") },
                { nameof(User.Age), Schema.Number().NonNegative().WithMessage("Age must be non-negative") },
                { nameof(User.IsActive), Schema.Boolean() },
                { nameof(User.Tags), Schema.Array<string>(Schema.String()).MinLength(1).WithMessage("At least one tag is required") },
                { nameof(User.PhoneNumber), Schema.String().Pattern(@"^\+\d{1,3}-\d{10}$").WithMessage("Phone number must be in format: +X-XXXXXXXXXX") },
                { nameof(User.Address), Schema.ObjectAsValidator<Address>(addressSchema) }
            };

            var validator = new ObjectValidator<User>(schema);

            // Make phone number optional by default
            validator.MarkPropertyAsOptional(nameof(User.PhoneNumber));

            // Add conditional validation: Phone number must start with +1- for USA addresses
            validator.AddDependencyRule(
                nameof(User.PhoneNumber),
                nameof(User.PhoneNumber),
                $"{nameof(User.Address)}.{nameof(Address.Country)}",
                (user, phone, country) => country?.ToString() != "USA" || (phone?.ToString()?.StartsWith("+1-") ?? false),
                "Phone number must start with +1- for USA addresses");

            return validator;
        });
    }

    private static void PrintValidationResult<T>(ValidationResult<T> result)
    {
        if (result.IsValid)
        {
            Console.WriteLine("Validation successful!");
        }
        else
        {
            Console.WriteLine("Validation failed with the following errors:");
            foreach (var error in result.Errors)
            {
                Console.WriteLine($"- {error.Message} {(error.PropertyName != null ? $"(Property: {error.PropertyName})" : "")}");
            }
        }
    }
} 