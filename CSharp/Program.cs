using System;
using System.Collections.Generic;
using SchemaValidation.Core;
using SchemaValidation.Models;

namespace SchemaValidation
{
    public class Program
    {
        public static void Main()
        {
            // Define address schema
            var addressSchema = Schema.Object<Address>(new Dictionary<string, Validator<object>>
            {
                { "Street", Schema.String() },
                { "City", Schema.String() },
                { "PostalCode", Schema.String().Pattern(@"^\d{5}$").WithMessage("Postal code must be 5 digits") },
                { "Country", Schema.String() }
            });

            // Define user schema
            var userSchema = Schema.Object<User>(new Dictionary<string, Validator<object>>
            {
                { "Id", Schema.String().WithMessage("ID must be a string") },
                { "Name", Schema.String().MinLength(2).MaxLength(50) },
                { "Email", Schema.String().Pattern(@"^[^\s@]+@[^\s@]+\.[^\s@]+$") },
                { "Age", Schema.Number() },
                { "IsActive", Schema.Boolean() },
                { "Tags", Schema.Array(Schema.String()) }
            });

            // Example data
            var userData = new User
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
            };

            // Validate data
            var result = userSchema.Validate(userData);
            Console.WriteLine($"Validation result: {(result.IsValid ? "Valid" : "Invalid")}");
            if (!result.IsValid)
            {
                Console.WriteLine($"Error: {result.ErrorMessage}");
            }
        }
    }
} 